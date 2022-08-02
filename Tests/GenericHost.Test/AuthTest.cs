namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AuthorizationEndpoint.Contract;
    using AuthorizationEndpoint.Host;
    using AuthorizationEndpoint.JwtAuthentication;
    using Basics;
    using CompositionRoot.Api.Abstractions.Registration;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Orm.Connection;
    using DataAccess.Orm.Host;
    using DataAccess.Orm.PostgreSql.Host;
    using DataAccess.Orm.Sql.Settings;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.DataAccess.EventSourcing;
    using GenericEndpoint.Host;
    using IntegrationTransport.Host;
    using IntegrationTransport.RabbitMQ.Settings;
    using IntegrationTransport.RpcRequest;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Migrations;
    using Mocks;
    using Overrides;
    using Registrations;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
    using TracingEndpoint.Host;
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
        /// <param name="fixture">ModulesTestFixture</param>
        public AuthTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// useContainer; useTransport; collector; databaseProvider; timeout;
        /// </summary>
        /// <returns>RunHostWithDataAccessTestData</returns>
        public static IEnumerable<object[]> RunHostWithDataAccessAndIntegrationTransportTracingTestData()
        {
            var timeout = TimeSpan.FromSeconds(60);

            var commonAppSettingsJson = SolutionExtensions
               .ProjectFile()
               .Directory
               .EnsureNotNull("Project directory not found")
               .StepInto("Settings")
               .GetFile("appsettings", ".json")
               .FullName;

            var useInMemoryIntegrationTransport = new Func<EndpointIdentity, string, IsolationLevel, ILogger, IHostBuilder, IHostBuilder>(
                (transportEndpointIdentity, settingsScope, isolationLevel, logger, hostBuilder) => hostBuilder
                   .ConfigureAppConfiguration(builder => builder.AddJsonFile(commonAppSettingsJson))
                   .UseIntegrationTransport(transportEndpointIdentity,
                        builder => builder
                           .WithInMemoryIntegrationTransport(hostBuilder)
                           .WithTracing()
                           .ModifyContainerOptions(options => options
                               .WithManualRegistrations(new MessagesCollectorManualRegistration())
                               .WithManualRegistrations(new AnonymousUserScopeProviderManualRegistration())
                               .WithManualRegistrations(new VirtualHostManualRegistration(settingsScope + isolationLevel))
                               .WithOverrides(new TestLoggerOverride(logger))
                               .WithOverrides(new TestSettingsScopeProviderOverride(settingsScope)))
                           .BuildOptions()));

            var useRabbitMqIntegrationTransport = new Func<EndpointIdentity, string, IsolationLevel, ILogger, IHostBuilder, IHostBuilder>(
                (transportEndpointIdentity, settingsScope, isolationLevel, logger, hostBuilder) => hostBuilder
                   .ConfigureAppConfiguration(builder => builder.AddJsonFile(commonAppSettingsJson))
                   .UseIntegrationTransport(transportEndpointIdentity,
                        builder => builder
                           .WithRabbitMqIntegrationTransport(hostBuilder)
                           .WithTracing()
                           .ModifyContainerOptions(options => options
                               .WithManualRegistrations(new MessagesCollectorManualRegistration())
                               .WithManualRegistrations(new AnonymousUserScopeProviderManualRegistration())
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

            var issuer = "Test";
            var audience = "Test";

            // var privateKey = Convert.ToBase64String(new HMACSHA256().Key)
            var privateKey = "db3OIsj+BXE9NZDy0t8W3TcNekrF+2d/1sFnWG4HnV8TZY30iTOdtVWJG8abWvB1GlOgJuQZdcF2Luqm/hccMw==";

            var configuration = new JwtAuthenticationConfiguration(issuer, audience, privateKey);
            var tokenProvider = new JwtTokenProvider(configuration);

            var token = tokenProvider.GenerateToken(username, TimeSpan.FromMinutes(5));
            Output.WriteLine(token);

            Assert.Equal(username, tokenProvider.GetUsername(token));
        }

        [Fact]
        internal void BasicAuthTest()
        {
            var username = "qwerty";
            var password = "1234";

            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            Output.WriteLine(token);

            var credentialBytes = Convert.FromBase64String(token);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);

            Assert.Equal(username, credentials[0]);
            Assert.Equal(password, credentials[1]);
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostWithDataAccessAndIntegrationTransportTracingTestData))]
        internal async Task AuthorizeUserTest(
            Func<EndpointIdentity, string, IsolationLevel, ILogger, IHostBuilder, IHostBuilder> useTransport,
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
                typeof(RecreatePostgreSqlDatabaseManualMigration)
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
                    new EndpointIdentity(TransportEndpointIdentity.LogicalName, Guid.NewGuid()),
                    settingsScope,
                    isolationLevel,
                    logger,
                    Fixture.CreateHostBuilder(Output))
               .UseAuthorizationEndpoint(0, builder => builder
                   .WithDataAccess(databaseProvider)
                   .WithTracing()
                   .ModifyContainerOptions(options => options
                       .WithManualRegistrations(manualRegistrations)
                       .WithOverrides(overrides))
                   .BuildOptions())
               .UseTracingEndpoint(0, builder => builder
                   .WithDataAccess(databaseProvider)
                   .ModifyContainerOptions(options => options
                       .WithManualRegistrations(manualRegistrations)
                       .WithOverrides(overrides))
                   .BuildOptions())
               .ExecuteMigrations(builder => builder
                   .WithDataAccess(databaseProvider)
                   .ModifyContainerOptions(options => options
                       .WithAdditionalOurTypes(manualMigrations)
                       .WithManualRegistrations(manualRegistrations)
                       .WithOverrides(overrides))
                   .BuildOptions())
               .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var endpointDependencyContainer = host.GetEndpointDependencyContainer(new EndpointIdentity(AuthorizationEndpointIdentity.LogicalName, 0));
            var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var sqlDatabaseSettings = await endpointDependencyContainer
                   .Resolve<ISettingsProvider<SqlDatabaseSettings>>()
                   .Get(cts.Token)
                   .ConfigureAwait(false);

                Assert.Equal(settingsScope, sqlDatabaseSettings.Database);
                Assert.Equal(isolationLevel, sqlDatabaseSettings.IsolationLevel);
                Assert.Equal(1u, sqlDatabaseSettings.ConnectionPoolSize);

                var rabbitMqSettings = await transportDependencyContainer
                   .Resolve<ISettingsProvider<RabbitMqSettings>>()
                   .Get(cts.Token)
                   .ConfigureAwait(false);

                Assert.Equal(virtualHost, rabbitMqSettings.VirtualHost);

                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var hostShutdown = host.WaitForShutdownAsync(cts.Token);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var username = "qwerty";
                var password = "1234";

                var query = new AuthorizeUser(username, password);
                UserAuthorizationResult? authorizationResult;

                await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                    var awaiter = Task.WhenAny(
                        hostShutdown,
                        integrationContext.RpcRequest<AuthorizeUser, UserAuthorizationResult>(query, CancellationToken.None));

                    var result = await awaiter.ConfigureAwait(false);

                    if (hostShutdown == result)
                    {
                        throw new InvalidOperationException("Host was unexpectedly stopped");
                    }

                    authorizationResult = await ((Task<UserAuthorizationResult>)result).ConfigureAwait(false);
                }

                Output.WriteLine(authorizationResult.ShowProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty));

                Assert.Equal(username, authorizationResult.Username);
                Assert.Empty(authorizationResult.Token);
                Assert.NotEmpty(authorizationResult.Details);

                await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                    var awaiter = Task.WhenAny(
                        hostShutdown,
                        Task.WhenAll(
                            collector.WaitUntilMessageIsNotReceived<CreateUser>(),
                            collector.WaitUntilMessageIsNotReceived<CaptureDomainEvent<AuthorizationEndpoint.Domain.UserCreated>>(),
                            collector.WaitUntilMessageIsNotReceived<UserCreated>()));

                    await integrationContext
                       .Send(new CreateUser(username, password), cts.Token)
                       .ConfigureAwait(false);

                    var result = await awaiter.ConfigureAwait(false);

                    if (hostShutdown == result)
                    {
                        throw new InvalidOperationException("Host was unexpectedly stopped");
                    }

                    await result.ConfigureAwait(false);
                }

                await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                    var awaiter = Task.WhenAny(
                        hostShutdown,
                        integrationContext.RpcRequest<AuthorizeUser, UserAuthorizationResult>(query, CancellationToken.None));

                    var result = await awaiter.ConfigureAwait(false);

                    if (hostShutdown == result)
                    {
                        throw new InvalidOperationException("Host was unexpectedly stopped");
                    }

                    authorizationResult = await ((Task<UserAuthorizationResult>)result).ConfigureAwait(false);
                }

                Output.WriteLine(authorizationResult.ShowProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty));

                Assert.Equal(username, authorizationResult.Username);
                Assert.NotEmpty(authorizationResult.Token);
                Assert.Empty(authorizationResult.Details);

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                await hostShutdown.ConfigureAwait(false);
            }
        }
    }
}