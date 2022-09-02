namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using AuthEndpoint.Contract;
    using AuthEndpoint.Contract.Commands;
    using AuthEndpoint.Contract.Queries;
    using AuthEndpoint.Contract.Replies;
    using AuthEndpoint.Host;
    using Basics;
    using CompositionRoot.Registration;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Orm.Connection;
    using DataAccess.Orm.PostgreSql.Host;
    using DataAccess.Orm.Sql.Settings;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.DataAccess.EventSourcing;
    using GenericEndpoint.Host;
    using IntegrationTransport.Host;
    using IntegrationTransport.RabbitMQ.Settings;
    using IntegrationTransport.RpcRequest;
    using JwtAuthentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Migrations;
    using Mocks;
    using Overrides;
    using Registrations;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
    using Web.Auth.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// AuthTest
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
    public class AuthTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public AuthTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// useContainer; useTransport; collector; databaseProvider; timeout;
        /// </summary>
        /// <returns>AuthTestData</returns>
        public static IEnumerable<object[]> AuthTestData()
        {
            var timeout = TimeSpan.FromSeconds(60);

            var settingsDirectory = SolutionExtensions
               .ProjectFile()
               .Directory
               .EnsureNotNull("Project directory wasn't found")
               .StepInto("Settings")
               .StepInto(nameof(AuthorizeUserTest));

            var useInMemoryIntegrationTransport = new Func<string, IsolationLevel, ILogger, IHostBuilder, IHostBuilder>(
                (settingsScope, isolationLevel, logger, hostBuilder) => hostBuilder
                   .UseIntegrationTransport(builder => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .ModifyContainerOptions(options => options
                           .WithManualRegistrations(new MessagesCollectorManualRegistration())
                           .WithManualRegistrations(new VirtualHostManualRegistration(settingsScope + isolationLevel))
                           .WithOverrides(new TestLoggerOverride(logger))
                           .WithOverrides(new TestSettingsScopeProviderOverride(settingsScope)))
                       .BuildOptions()));

            var useRabbitMqIntegrationTransport = new Func<string, IsolationLevel, ILogger, IHostBuilder, IHostBuilder>(
                (settingsScope, isolationLevel, logger, hostBuilder) => hostBuilder
                   .UseIntegrationTransport(builder => builder
                       .WithRabbitMqIntegrationTransport(hostBuilder)
                       .ModifyContainerOptions(options => options
                           .WithManualRegistrations(new MessagesCollectorManualRegistration())
                           .WithManualRegistrations(new VirtualHostManualRegistration(settingsScope + isolationLevel))
                           .WithOverrides(new TestLoggerOverride(logger))
                           .WithOverrides(new TestSettingsScopeProviderOverride(settingsScope)))
                       .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                useInMemoryIntegrationTransport,
                useRabbitMqIntegrationTransport
            };

            var databaseProviders = new IDatabaseProvider[]
            {
                new PostgreSqlDatabaseProvider()
            };

            var isolationLevels = new[]
            {
                IsolationLevel.Snapshot,
                IsolationLevel.ReadCommitted
            };

            return integrationTransportProviders
               .SelectMany(useTransport => databaseProviders
                   .SelectMany(databaseProvider => isolationLevels
                       .Select(isolationLevel => new object[]
                       {
                           settingsDirectory,
                           useTransport,
                           databaseProvider,
                           isolationLevel,
                           timeout
                       })));
        }

        [Fact]
        internal void JwtTokenProviderTest()
        {
            var username = "qwerty";

            var authEndpointConfigurationFilePath = SolutionExtensions
               .SolutionFile()
               .Directory
               .EnsureNotNull("Solution directory wasn't found")
               .StepInto("Tests")
               .StepInto("Test.WebApplication")
               .StepInto("Settings")
               .StepInto(AuthEndpointIdentity.LogicalName)
               .GetFile("appsettings", ".json")
               .FullName;

            var authEndpointConfiguration = new ConfigurationBuilder()
               .AddJsonFile(authEndpointConfigurationFilePath)
               .Build();

            var tokenProvider = new JwtTokenProvider(authEndpointConfiguration.GetJwtAuthenticationConfiguration());

            var token = tokenProvider.GenerateToken(username, TimeSpan.FromMinutes(5));
            Output.WriteLine(token);

            Assert.Equal(username, tokenProvider.GetUsername(token));
        }

        [Fact]
        internal void BasicAuthTest()
        {
            var username = "qwerty";
            var password = "12345678";

            var token = (username, password).EncodeBasicAuth();
            var credentials = token.DecodeBasicAuth();

            Output.WriteLine(token);

            Assert.Equal(username, credentials.username);
            Assert.Equal(password, credentials.password);
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(AuthTestData))]
        internal async Task AuthorizeUserTest(
            DirectoryInfo settingsDirectory,
            Func<string, IsolationLevel, ILogger, IHostBuilder, IHostBuilder> useTransport,
            IDatabaseProvider databaseProvider,
            IsolationLevel isolationLevel,
            TimeSpan timeout)
        {
            Output.WriteLine(databaseProvider.GetType().FullName);
            Output.WriteLine(isolationLevel.ToString());

            var logger = Fixture.CreateLogger(Output);

            var settingsScope = nameof(AuthorizeUserTest);
            var virtualHost = settingsScope + isolationLevel;

            var manualMigrations = new[]
            {
                typeof(RecreatePostgreSqlDatabaseMigration)
            };

            var manualRegistrations = new IManualRegistration[]
            {
                new IsolationLevelManualRegistration(isolationLevel)
            };

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(settingsScope)
            };

            var host = useTransport(
                    settingsScope,
                    isolationLevel,
                    logger,
                    Fixture.CreateHostBuilder(Output))
               .UseAuthEndpoint(builder => builder
                   .WithDataAccess(databaseProvider,
                        options => options
                           .ExecuteMigrations())
                   .ModifyContainerOptions(options => options
                       .WithAdditionalOurTypes(manualMigrations)
                       .WithManualRegistrations(manualRegistrations)
                       .WithOverrides(overrides))
                   .BuildOptions())
               .BuildHost(settingsDirectory);

            await RunHostTest.RunTestHost(
                    Output,
                    host,
                    AuthorizeUserTestInternal(settingsScope, virtualHost, isolationLevel),
                    timeout)
               .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> AuthorizeUserTestInternal(
                string settingsScope,
                string virtualHost,
                IsolationLevel isolationLevel)
            {
                return async (output, host, token) =>
                {
                    var transportDependencyContainer = host.GetTransportDependencyContainer();
                    var endpointDependencyContainer = host.GetEndpointDependencyContainer(AuthEndpointIdentity.LogicalName);
                    var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                    var sqlDatabaseSettings = await endpointDependencyContainer
                       .Resolve<ISettingsProvider<SqlDatabaseSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(settingsScope, sqlDatabaseSettings.Database);
                    Assert.Equal(isolationLevel, sqlDatabaseSettings.IsolationLevel);
                    Assert.Equal(1u, sqlDatabaseSettings.ConnectionPoolSize);

                    var rabbitMqSettings = await transportDependencyContainer
                       .Resolve<ISettingsProvider<RabbitMqSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(virtualHost, rabbitMqSettings.VirtualHost);

                    var username = "qwerty";
                    var password = "12345678";

                    var query = new AuthorizeUser(username, password);
                    UserAuthorizationResult? authorizationResult;

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                        authorizationResult = await integrationContext
                           .RpcRequest<AuthorizeUser, UserAuthorizationResult>(query, CancellationToken.None)
                           .ConfigureAwait(false);
                    }

                    output.WriteLine(authorizationResult.ShowProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty));

                    Assert.Equal(username, authorizationResult.Username);
                    Assert.Empty(authorizationResult.Token);
                    Assert.NotEmpty(authorizationResult.Details);

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                        var awaiter = Task.WhenAll(
                            collector.WaitUntilMessageIsNotReceived<CreateUser>(),
                            collector.WaitUntilMessageIsNotReceived<CaptureDomainEvent<AuthEndpoint.Domain.Model.UserCreated>>(),
                            collector.WaitUntilMessageIsNotReceived<AuthEndpoint.Contract.Events.UserCreated>());

                        await integrationContext
                           .Send(new CreateUser(username, password), token)
                           .ConfigureAwait(false);

                        await awaiter.ConfigureAwait(false);
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                        authorizationResult = await integrationContext
                           .RpcRequest<AuthorizeUser, UserAuthorizationResult>(query, CancellationToken.None)
                           .ConfigureAwait(false);
                    }

                    output.WriteLine(authorizationResult.ShowProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty));

                    Assert.Equal(username, authorizationResult.Username);
                    Assert.NotEmpty(authorizationResult.Token);
                    Assert.Empty(authorizationResult.Details);
                };
            }
        }
    }
}