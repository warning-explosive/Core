namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using CrossCuttingConcerns.Json;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Api.Exceptions;
    using DataAccess.Api.Persisting;
    using DataAccess.Api.Transaction;
    using DataAccess.Orm.Connection;
    using DataAccess.Orm.Host;
    using DataAccess.Orm.PostgreSql.Extensions;
    using DataAccess.Orm.PostgreSql.Host;
    using DataAccess.Orm.Sql.Settings;
    using DatabaseEntities;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.DataAccess.Settings;
    using GenericEndpoint.Host;
    using GenericEndpoint.Messaging.MessageHeaders;
    using IntegrationTransport.Host;
    using IntegrationTransport.RabbitMQ.Settings;
    using IntegrationTransport.RpcRequest;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Migrations;
    using Mocks;
    using Overrides;
    using Registrations;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
    using TracingEndpoint.Contract;
    using TracingEndpoint.Host;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// DataAccessTest
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
    public class DataAccessTest : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public DataAccessTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// useContainer; useTransport; collector; databaseProvider; timeout;
        /// </summary>
        /// <returns>RunHostWithDataAccessTestData</returns>
        public static IEnumerable<object[]> RunHostWithDataAccessTestData()
        {
            var timeout = TimeSpan.FromSeconds(60);

            var commonAppSettingsJson = SolutionExtensions
               .ProjectFile()
               .Directory
               .EnsureNotNull("Project directory not found")
               .StepInto("Settings")
               .GetFile("appsettings", ".json")
               .FullName;

            var useInMemoryIntegrationTransport = new Func<string, IsolationLevel, ILogger, IHostBuilder, IHostBuilder>(
                (settingsScope, isolationLevel, logger, hostBuilder) => hostBuilder
                   .ConfigureAppConfiguration(builder => builder.AddJsonFile(commonAppSettingsJson))
                   .UseIntegrationTransport(builder => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .ModifyContainerOptions(options => options
                           .WithManualRegistrations(new MessagesCollectorManualRegistration())
                           .WithManualRegistrations(new AnonymousUserScopeProviderManualRegistration())
                           .WithManualRegistrations(new VirtualHostManualRegistration(settingsScope + isolationLevel))
                           .WithOverrides(new TestLoggerOverride(logger))
                           .WithOverrides(new TestSettingsScopeProviderOverride(settingsScope)))
                       .BuildOptions()));

            var useRabbitMqIntegrationTransport = new Func<string, IsolationLevel, ILogger, IHostBuilder, IHostBuilder>(
                (settingsScope, isolationLevel, logger, hostBuilder) => hostBuilder
                   .ConfigureAppConfiguration(builder => builder.AddJsonFile(commonAppSettingsJson))
                   .UseIntegrationTransport(builder => builder
                       .WithRabbitMqIntegrationTransport(hostBuilder)
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

            var useInMemoryIntegrationTransport = new Func<string, IsolationLevel, ILogger, IHostBuilder, IHostBuilder>(
                (settingsScope, isolationLevel, logger, hostBuilder) => hostBuilder
                   .ConfigureAppConfiguration(builder => builder.AddJsonFile(commonAppSettingsJson))
                   .UseIntegrationTransport(builder => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .WithTracing()
                       .ModifyContainerOptions(options => options
                           .WithManualRegistrations(new MessagesCollectorManualRegistration())
                           .WithManualRegistrations(new AnonymousUserScopeProviderManualRegistration())
                           .WithManualRegistrations(new VirtualHostManualRegistration(settingsScope + isolationLevel))
                           .WithOverrides(new TestLoggerOverride(logger))
                           .WithOverrides(new TestSettingsScopeProviderOverride(settingsScope)))
                       .BuildOptions()));

            var useRabbitMqIntegrationTransport = new Func<string, IsolationLevel, ILogger, IHostBuilder, IHostBuilder>(
                (settingsScope, isolationLevel, logger, hostBuilder) => hostBuilder
                   .ConfigureAppConfiguration(builder => builder.AddJsonFile(commonAppSettingsJson))
                   .UseIntegrationTransport(builder => builder
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

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostWithDataAccessAndIntegrationTransportTracingTestData))]
        internal async Task GetConversationTraceTest(
            Func<string, IsolationLevel, ILogger, IHostBuilder, IHostBuilder> useTransport,
            IDatabaseProvider databaseProvider,
            IsolationLevel isolationLevel,
            TimeSpan timeout)
        {
            Output.WriteLine(databaseProvider.GetType().FullName);
            Output.WriteLine(isolationLevel.ToString());

            var logger = Fixture.CreateLogger(Output);

            var messageTypes = new[]
            {
                typeof(Query),
                typeof(Reply)
            };

            var messageHandlerTypes = new[]
            {
                typeof(QueryAlwaysReplyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var settingsScope = nameof(GetConversationTraceTest);
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
                    settingsScope,
                    isolationLevel,
                    logger,
                    Fixture.CreateHostBuilder(Output))
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .WithDataAccess(databaseProvider)
                       .WithTracing()
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithManualRegistrations(manualRegistrations)
                           .WithOverrides(overrides))
                       .BuildOptions())
               .UseTracingEndpoint(TestIdentity.Instance0,
                    builder => builder
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
            var endpointDependencyContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);
            var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

            var jsonSerializer = host
               .GetEndpointDependencyContainer(TestIdentity.Endpoint10)
               .Resolve<IJsonSerializer>();

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

                var conversationId = Guid.NewGuid();

                var awaiter = Task.WhenAny(
                    hostShutdown,
                    Task.WhenAll(
                        collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(GetConversationTrace)),
                        collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(ConversationTrace))));

                await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                    var trace = await integrationContext
                        .RpcRequest<GetConversationTrace, ConversationTrace>(new GetConversationTrace(conversationId), cts.Token)
                        .ConfigureAwait(false);

                    var result = await awaiter.ConfigureAwait(false);

                    if (hostShutdown == result)
                    {
                        throw new InvalidOperationException("Host was unexpectedly stopped");
                    }

                    await result.ConfigureAwait(false);

                    Assert.Empty(collector.ErrorMessages);
                    Assert.Equal(4, collector.Messages.Count);
                    var messages = collector.Messages.ToArray();
                    collector.Messages.Clear();

                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(GetConversationTrace)));
                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(ConversationTrace)));
                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(CaptureTrace) && ((CaptureTrace)message.Payload).SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(GetConversationTrace)));
                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(CaptureTrace) && ((CaptureTrace)message.Payload).SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(ConversationTrace)));

                    Assert.Equal(conversationId, trace.ConversationId);
                    Assert.Null(trace.SerializedMessage);
                    Assert.Null(trace.RefuseReason);
                    Assert.NotNull(trace.SubsequentTrace);
                    Assert.Empty(trace.SubsequentTrace);

                    awaiter = Task.WhenAny(
                        hostShutdown,
                        Task.WhenAll(
                            collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(Query)),
                            collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(Reply))));

                    var reply = await integrationContext
                        .RpcRequest<Query, Reply>(new Query(42), cts.Token)
                        .ConfigureAwait(false);

                    result = await awaiter.ConfigureAwait(false);

                    if (hostShutdown == result)
                    {
                        throw new InvalidOperationException("Host was unexpectedly stopped");
                    }

                    await result.ConfigureAwait(false);

                    Assert.Empty(collector.ErrorMessages);
                    Assert.Equal(4, collector.Messages.Count);
                    messages = collector.Messages.ToArray();
                    collector.Messages.Clear();

                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(Query)));
                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(Reply)));
                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(CaptureTrace) && ((CaptureTrace)message.Payload).SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(Query)));
                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(CaptureTrace) && ((CaptureTrace)message.Payload).SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(Reply)));

                    conversationId = messages[0].ReadRequiredHeader<ConversationId>().Value;

                    awaiter = Task.WhenAny(
                        hostShutdown,
                        Task.WhenAll(
                            collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(GetConversationTrace)),
                            collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(ConversationTrace))));

                    trace = await integrationContext
                        .RpcRequest<GetConversationTrace, ConversationTrace>(new GetConversationTrace(conversationId), cts.Token)
                        .ConfigureAwait(false);

                    result = await awaiter.ConfigureAwait(false);

                    if (hostShutdown == result)
                    {
                        throw new InvalidOperationException("Host was unexpectedly stopped");
                    }

                    await result.ConfigureAwait(false);

                    Assert.Empty(collector.ErrorMessages);
                    Assert.Equal(4, collector.Messages.Count);
                    collector.Messages.Clear();

                    Assert.Equal(conversationId, trace.ConversationId);
                    Assert.NotNull(trace.SerializedMessage);
                    Assert.NotEmpty(trace.SerializedMessage.Payload);
                    Assert.Equal(typeof(Query), trace.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType);
                    Assert.Equal(42, ((Query)trace.SerializedMessage.ToIntegrationMessage(jsonSerializer).Payload).Id);
                    Assert.Null(trace.RefuseReason);
                    Assert.NotNull(trace.SubsequentTrace);
                    Assert.Single(trace.SubsequentTrace);

                    var subsequentTrace = trace.SubsequentTrace.Single();
                    Assert.NotNull(subsequentTrace.SerializedMessage);
                    Assert.NotEmpty(subsequentTrace.SerializedMessage.Payload);
                    Assert.Equal(typeof(Reply), subsequentTrace.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType);
                    Assert.Equal(reply.Id, ((Reply)subsequentTrace.SerializedMessage.ToIntegrationMessage(jsonSerializer).Payload).Id);
                    Assert.Null(subsequentTrace.RefuseReason);
                    Assert.NotNull(subsequentTrace.SubsequentTrace);
                    Assert.Empty(subsequentTrace.SubsequentTrace);

                    await host.StopAsync(cts.Token).ConfigureAwait(false);

                    await hostShutdown.ConfigureAwait(false);
                }
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostWithDataAccessTestData))]
        internal async Task BackgroundOutboxDeliveryTest(
            Func<string, IsolationLevel, ILogger, IHostBuilder, IHostBuilder> useTransport,
            IDatabaseProvider databaseProvider,
            IsolationLevel isolationLevel,
            TimeSpan timeout)
        {
            Output.WriteLine(databaseProvider.GetType().FullName);
            Output.WriteLine(isolationLevel.ToString());

            var logger = Fixture.CreateLogger(Output);

            var messageTypes = new[]
            {
                typeof(Query),
                typeof(Reply)
            };

            var messageHandlerTypes = new[]
            {
                typeof(QueryAlwaysReplyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var manualMigrations = new[]
            {
                typeof(RecreatePostgreSqlDatabaseManualMigration)
            };

            var settingsScope = nameof(BackgroundOutboxDeliveryTest);
            var virtualHost = settingsScope + isolationLevel;

            var endpointManualRegistrations = new IManualRegistration[]
            {
                new BackgroundOutboxDeliveryManualRegistration(),
                new IsolationLevelManualRegistration(isolationLevel)
            };

            var migrationManualRegistrations = new IManualRegistration[]
            {
                new IsolationLevelManualRegistration(isolationLevel)
            };

            var endpointOverrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(settingsScope)
            };

            var migrationOverrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(settingsScope)
            };

            var host = useTransport(
                    settingsScope,
                    isolationLevel,
                    logger,
                    Fixture.CreateHostBuilder(Output))
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .WithDataAccess(databaseProvider)
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithManualRegistrations(endpointManualRegistrations)
                           .WithOverrides(endpointOverrides))
                       .BuildOptions())
               .ExecuteMigrations(builder => builder
                   .WithDataAccess(databaseProvider)
                   .ModifyContainerOptions(options => options
                       .WithAdditionalOurTypes(manualMigrations)
                       .WithManualRegistrations(migrationManualRegistrations)
                       .WithOverrides(migrationOverrides))
                   .BuildOptions())
               .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var endpointDependencyContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

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

                Reply reply;

                var outboxSettings = await endpointDependencyContainer
                   .Resolve<ISettingsProvider<OutboxSettings>>()
                   .Get(cts.Token)
                   .ConfigureAwait(false);

                Assert.Equal(TimeSpan.FromSeconds(1), outboxSettings.OutboxDeliveryInterval);

                await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                    var awaiter = Task.WhenAny(
                        hostShutdown,
                        integrationContext.RpcRequest<Query, Reply>(new Query(42), cts.Token));

                    var result = await awaiter.ConfigureAwait(false);

                    if (hostShutdown == result)
                    {
                        throw new InvalidOperationException("Host was unexpectedly stopped");
                    }

                    reply = await ((Task<Reply>)result).ConfigureAwait(false);
                }

                Assert.Equal(42, reply.Id);

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                await hostShutdown.ConfigureAwait(false);
            }
        }

        [SuppressMessage("Analysis", "xUnit1004", Justification = "#133")]
        [Theory(Timeout = 60_000, Skip = "#133")]
        [MemberData(nameof(RunHostWithDataAccessTestData))]
        internal async Task OptimisticConcurrencyTest(
            Func<string, IsolationLevel, ILogger, IHostBuilder, IHostBuilder> useTransport,
            IDatabaseProvider databaseProvider,
            IsolationLevel isolationLevel,
            TimeSpan timeout)
        {
            Output.WriteLine(databaseProvider.GetType().FullName);
            Output.WriteLine(isolationLevel.ToString());

            var logger = Fixture.CreateLogger(Output);

            var additionalOurTypes = new[]
            {
                typeof(DatabaseEntity)
            };

            var settingsScope = nameof(OptimisticConcurrencyTest);
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
                    settingsScope,
                    isolationLevel,
                    logger,
                    Fixture.CreateHostBuilder(Output))
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .WithDataAccess(databaseProvider)
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
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
            var endpointDependencyContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var sqlDatabaseSettings = await endpointDependencyContainer
                   .Resolve<ISettingsProvider<SqlDatabaseSettings>>()
                   .Get(cts.Token)
                   .ConfigureAwait(false);

                Assert.Equal(settingsScope, sqlDatabaseSettings.Database);
                Assert.Equal(isolationLevel, sqlDatabaseSettings.IsolationLevel);
                Assert.Equal(3u, sqlDatabaseSettings.ConnectionPoolSize);

                var rabbitMqSettings = await transportDependencyContainer
                   .Resolve<ISettingsProvider<RabbitMqSettings>>()
                   .Get(cts.Token)
                   .ConfigureAwait(false);

                Assert.Equal(virtualHost, rabbitMqSettings.VirtualHost);

                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var hostShutdown = host.WaitForShutdownAsync(cts.Token);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var primaryKey = Guid.NewGuid();
                var delay = TimeSpan.FromMilliseconds(100);

                // #1 - create/create
                {
                    Exception? exception = null;

                    try
                    {
                        await Task.WhenAll(
                                CreateEntity(endpointDependencyContainer, primaryKey, cts.Token),
                                CreateEntity(endpointDependencyContainer, primaryKey, cts.Token))
                           .ConfigureAwait(false);
                    }
                    catch (DatabaseException databaseException) when (databaseException is not DatabaseConcurrentUpdateException)
                    {
                        exception = databaseException;
                    }

                    Assert.NotNull(exception);
                    Assert.NotNull(exception.InnerException);
                    Assert.True(exception.InnerException!.IsUniqueViolation());
                }

                // #2 - update/update
                {
                    Exception? exception = null;

                    try
                    {
                        await Task.WhenAll(
                                UpdateEntity(endpointDependencyContainer, primaryKey, delay, cts.Token),
                                UpdateEntity(endpointDependencyContainer, primaryKey, TimeSpan.Zero, cts.Token))
                           .ConfigureAwait(false);
                    }
                    catch (DatabaseConcurrentUpdateException concurrentUpdateException)
                    {
                        exception = concurrentUpdateException;
                    }

                    Assert.NotNull(exception);
                }

                // #3 - update/delete
                {
                    Exception? exception = null;

                    try
                    {
                        await Task.WhenAll(
                                UpdateEntity(endpointDependencyContainer, primaryKey, delay, cts.Token),
                                DeleteEntity(endpointDependencyContainer, primaryKey, cts.Token))
                           .ConfigureAwait(false);
                    }
                    catch (DatabaseConcurrentUpdateException concurrentUpdateException)
                    {
                        exception = concurrentUpdateException;
                    }

                    Assert.NotNull(exception);
                }

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                await hostShutdown.ConfigureAwait(false);
            }

            static async Task CreateEntity(
                IDependencyContainer dependencyContainer,
                Guid primaryKey,
                CancellationToken token)
            {
                await using (dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var transaction = dependencyContainer.Resolve<IDatabaseTransaction>();

                    await using (await transaction.OpenScope(true, token).ConfigureAwait(false))
                    {
                        var entity = new DatabaseEntity(
                            primaryKey,
                            true,
                            "SomeString",
                            "SomeNullableString",
                            42);

                        var inserted = await transaction
                           .Write<DatabaseEntity, Guid>()
                           .Insert(new[] { entity }, EnInsertBehavior.Default, token)
                           .ConfigureAwait(false);

                        Assert.Equal(1, inserted);
                    }

                    dependencyContainer
                       .Resolve<ILogger>()
                       .Debug($"{nameof(CreateEntity)}: {primaryKey}");
                }
            }

            static async Task UpdateEntity(
                IDependencyContainer dependencyContainer,
                Guid primaryKey,
                TimeSpan delay,
                CancellationToken token)
            {
                await using (dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var transaction = dependencyContainer.Resolve<IDatabaseTransaction>();

                    await using (await transaction.OpenScope(true, token).ConfigureAwait(false))
                    {
                        _ = await transaction
                           .Write<DatabaseEntity, Guid>()
                           .Update(entity => entity.IntField,
                                entity => entity.IntField + 1,
                                entity => entity.PrimaryKey == primaryKey,
                                token)
                           .ConfigureAwait(false);

                        await Task
                           .Delay(delay, token)
                           .ConfigureAwait(false);
                    }

                    dependencyContainer
                       .Resolve<ILogger>()
                       .Debug($"{nameof(UpdateEntity)}: {primaryKey}");
                }
            }

            static async Task DeleteEntity(
                IDependencyContainer dependencyContainer,
                Guid primaryKey,
                CancellationToken token)
            {
                await using (dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var transaction = dependencyContainer.Resolve<IDatabaseTransaction>();

                    await using (await transaction.OpenScope(true, token).ConfigureAwait(false))
                    {
                        _ = await transaction
                           .Write<DatabaseEntity, Guid>()
                           .Delete(entity => entity.PrimaryKey == primaryKey, token)
                           .ConfigureAwait(false);
                    }

                    dependencyContainer
                       .Resolve<ILogger>()
                       .Debug($"{nameof(DeleteEntity)}: {primaryKey}");
                }
            }
        }
    }
}