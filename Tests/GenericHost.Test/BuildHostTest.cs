namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AuthorizationEndpoint.Contract;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using DataAccess.Api.Model;
    using DataAccess.Orm.Connection;
    using DataAccess.Orm.Host;
    using DataAccess.Orm.Host.Model;
    using DataAccess.Orm.PostgreSql.Host;
    using DataAccess.Orm.Sql.Host.Model;
    using DataAccess.Orm.Sql.Model;
    using DatabaseEntities.Relations;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.DataAccess.Deduplication;
    using GenericEndpoint.DataAccess.EventSourcing;
    using GenericEndpoint.Host;
    using GenericEndpoint.Host.StartupActions;
    using GenericEndpoint.Messaging.Abstractions;
    using GenericEndpoint.Pipeline;
    using GenericEndpoint.RpcRequest;
    using GenericEndpoint.Tracing.Pipeline;
    using GenericHost;
    using IntegrationTransport.Host;
    using IntegrationTransport.Host.BackgroundWorkers;
    using IntegrationTransport.Integration;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Overrides;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration;
    using SpaceEngineers.Core.CompositionRoot.Api.Exceptions;
    using SpaceEngineers.Core.CompositionRoot.Api.Extensions;
    using SpaceEngineers.Core.DataAccess.Orm.Host.Migrations;
    using SpaceEngineers.Core.GenericEndpoint.Api.Abstractions;
    using SpaceEngineers.Core.GenericEndpoint.DataAccess.BackgroundWorkers;
    using SpaceEngineers.Core.IntegrationTransport.Api.Abstractions;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
    using TracingEndpoint.Contract;
    using TracingEndpoint.DatabaseModel;
    using TracingEndpoint.Host;
    using Xunit;
    using Xunit.Abstractions;
    using EndpointIdentity = GenericEndpoint.Contract.EndpointIdentity;
    using IntegrationMessage = GenericEndpoint.Messaging.IntegrationMessage;

    /// <summary>
    /// BuildHostTest
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
    public class BuildHostTest : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public BuildHostTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// useContainer; useTransport;
        /// </summary>
        /// <returns>BuildHostTestData</returns>
        public static IEnumerable<object[]> BuildHostTestData()
        {
            var commonAppSettingsJson = SolutionExtensions
               .ProjectFile()
               .Directory
               .EnsureNotNull("Project directory not found")
               .StepInto("Settings")
               .GetFile("appsettings", ".json")
               .FullName;

            var useInMemoryIntegrationTransport = new Func<ILogger, IHostBuilder, IHostBuilder>(
                (logger, hostBuilder) => hostBuilder
                   .ConfigureAppConfiguration(builder => builder.AddJsonFile(commonAppSettingsJson))
                   .UseIntegrationTransport(builder => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .WithTracing()
                       .ModifyContainerOptions(options => options.WithOverrides(new TestLoggerOverride(logger)))
                       .BuildOptions()));

            var useRabbitMqIntegrationTransport = new Func<ILogger, IHostBuilder, IHostBuilder>(
                (logger, hostBuilder) => hostBuilder
                   .ConfigureAppConfiguration(builder => builder.AddJsonFile(commonAppSettingsJson))
                   .UseIntegrationTransport(builder => builder
                       .WithRabbitMqIntegrationTransport(hostBuilder)
                       .WithTracing()
                       .ModifyContainerOptions(options => options.WithOverrides(new TestLoggerOverride(logger)))
                       .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                useInMemoryIntegrationTransport,
                useRabbitMqIntegrationTransport
            };

            return integrationTransportProviders
               .Select(useTransport => new object[]
               {
                   useTransport
               });
        }

        /// <summary>
        /// useContainer; useTransport; databaseProvider;
        /// </summary>
        /// <returns>RunHostWithDataAccessTestData</returns>
        public static IEnumerable<object[]> BuildHostWithDataAccessTestData()
        {
            var commonAppSettingsJson = SolutionExtensions
               .ProjectFile()
               .Directory
               .EnsureNotNull("Project directory not found")
               .StepInto("Settings")
               .GetFile("appsettings", ".json")
               .FullName;

            var useInMemoryIntegrationTransport = new Func<string, ILogger, IHostBuilder, IHostBuilder>(
                (settingsScope, logger, hostBuilder) => hostBuilder
                   .ConfigureAppConfiguration(builder => builder.AddJsonFile(commonAppSettingsJson))
                   .UseIntegrationTransport(builder => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .ModifyContainerOptions(options => options
                           .WithOverrides(new TestLoggerOverride(logger))
                           .WithOverrides(new TestSettingsScopeProviderOverride(settingsScope)))
                       .BuildOptions()));

            var useRabbitMqIntegrationTransport = new Func<string, ILogger, IHostBuilder, IHostBuilder>(
                (settingsScope, logger, hostBuilder) => hostBuilder
                   .ConfigureAppConfiguration(builder => builder.AddJsonFile(commonAppSettingsJson))
                   .UseIntegrationTransport(builder => builder
                       .WithRabbitMqIntegrationTransport(hostBuilder)
                       .ModifyContainerOptions(options => options
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

            return integrationTransportProviders
               .SelectMany(useTransport => databaseProviders
                   .Select(databaseProvider => new object[]
                   {
                       useTransport,
                       databaseProvider
                   }));
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostTestData))]
        internal void SameTransportTest(Func<ILogger, IHostBuilder, IHostBuilder> useTransport)
        {
            var logger = Fixture.CreateLogger(Output);

            var host = useTransport(logger, Fixture.CreateHostBuilder(Output))
                .UseEndpoint(
                    TestIdentity.Endpoint10,
                    (_, builder) => builder
                        .WithTracing()
                        .ModifyContainerOptions(options => options.WithOverrides(new TestLoggerOverride(logger)))
                        .BuildOptions())
                .UseEndpoint(
                    TestIdentity.Endpoint20,
                    (_, builder) => builder
                        .WithTracing()
                        .ModifyContainerOptions(options => options.WithOverrides(new TestLoggerOverride(logger)))
                        .BuildOptions())
                .BuildHost();

            var integrationTransport = host.GetTransportDependencyContainer().Resolve<IIntegrationTransport>();
            Output.WriteLine($"{nameof(IIntegrationTransport)}: {integrationTransport.GetType().FullName}");

            var transportIsSame = new[]
                {
                    host.GetEndpointDependencyContainer(TestIdentity.Endpoint10),
                    host.GetEndpointDependencyContainer(TestIdentity.Endpoint20)
                }
                .Select(container => container.Resolve<IIntegrationTransport>())
                .All(endpointTransport => ReferenceEquals(integrationTransport, endpointTransport));

            Assert.True(transportIsSame);
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostTestData))]
        internal void BuildTest(Func<ILogger, IHostBuilder, IHostBuilder> useTransport)
        {
            var logger = Fixture.CreateLogger(Output);

            var messageTypes = new[]
            {
                typeof(BaseEvent),
                typeof(InheritedEvent),
                typeof(Command),
                typeof(OpenGenericHandlerCommand),
                typeof(Query),
                typeof(Reply)
            };

            var messageHandlerTypes = new[]
            {
                typeof(BaseEventEmptyMessageHandler),
                typeof(InheritedEventEmptyMessageHandler),
                typeof(CommandEmptyMessageHandler),
                typeof(OpenGenericCommandEmptyMessageHandler<>),
                typeof(QueryAlwaysReplyMessageHandler),
                typeof(ReplyEmptyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var databaseProvider = new PostgreSqlDatabaseProvider();

            var settingsScope = nameof(BuildTest);

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(settingsScope)
            };

            var host = useTransport(logger, Fixture.CreateHostBuilder(Output))
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .WithDataAccess(databaseProvider)
                       .WithTracing()
                       .ModifyContainerOptions(options => options
                           .WithOverrides(overrides)
                           .WithAdditionalOurTypes(additionalOurTypes))
                       .BuildOptions())
               .ExecuteMigrations(builder => builder
                   .WithDataAccess(databaseProvider)
                   .ModifyContainerOptions(options => options
                       .WithOverrides(overrides))
                   .BuildOptions())
               .BuildHost();

            using (host)
            {
                CheckHost(host);
                CheckEndpoint(host, TestIdentity.Endpoint10, Output.WriteLine);
                CheckTransport(host, Output.WriteLine);
            }

            static void CheckHost(IHost host)
            {
                _ = host.Services.GetRequiredService<IHostedService>();

                var expectedHostStartupActions = new[]
                {
                    typeof(UpgradeDatabaseHostStartupAction),
                    typeof(GenericEndpointHostStartupAction),
                    typeof(GenericEndpointHostStartupAction)
                };

                var actualHostStartupActions = host
                    .Services
                    .GetServices<IHostStartupAction>()
                    .Select(startup => startup.GetType())
                    .OrderBy(type => type.FullName)
                    .ToList();

                Assert.Equal(expectedHostStartupActions.OrderBy(type => type.FullName).ToList(), actualHostStartupActions);

                var expectedHostBackgroundWorkers = new[]
                {
                    typeof(GenericEndpointDataAccessHostBackgroundWorker),
                    typeof(IntegrationTransportHostBackgroundWorker)
                };

                var actualHostBackgroundWorkers = host
                    .Services
                    .GetServices<IHostBackgroundWorker>()
                    .Select(startup => startup.GetType())
                    .OrderBy(type => type.FullName)
                    .ToList();

                Assert.Equal(expectedHostBackgroundWorkers.OrderBy(type => type.FullName).ToList(), actualHostBackgroundWorkers);
            }

            static void CheckEndpoint(IHost host, EndpointIdentity endpointIdentity, Action<string> log)
            {
                log($"Endpoint: {endpointIdentity}");

                var endpointDependencyContainer = host.GetEndpointDependencyContainer(endpointIdentity);
                var integrationMessage = new IntegrationMessage(new Command(0), typeof(Command));

                Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage));
                Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IIntegrationContext>());

                using (endpointDependencyContainer.OpenScope())
                {
                    var expectedContexts = new[]
                    {
                        typeof(AdvancedIntegrationContext)
                    };

                    Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                    var advancedIntegrationContext = endpointDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage);

                    var actualAdvancedIntegrationContexts = advancedIntegrationContext
                        .FlattenDecoratedObject(obj => obj.GetType())
                        .ShowTypes("advanced integration context", log)
                        .ToList();

                    Assert.Equal(expectedContexts, actualAdvancedIntegrationContexts);

                    var integrationContext = endpointDependencyContainer.Resolve<IIntegrationContext>();

                    var actualIntegrationContexts = integrationContext
                        .FlattenDecoratedObject(obj => obj.GetType())
                        .ShowTypes("integration context", log)
                        .ToList();

                    Assert.Equal(expectedContexts, actualIntegrationContexts);

                    var expectedMessagesCollector = new[]
                    {
                        typeof(MessagesCollector)
                    };

                    var messagesCollector = endpointDependencyContainer.Resolve<IMessagesCollector>();

                    var actualMessagesCollector = messagesCollector
                       .FlattenDecoratedObject(obj => obj.GetType())
                       .ShowTypes("collector", log)
                       .ToList();

                    Assert.Equal(expectedMessagesCollector, actualMessagesCollector);

                    var expectedPipeline = new[]
                    {
                        typeof(ErrorHandlingPipeline),
                        typeof(TracingPipeline),
                        typeof(UnitOfWorkPipeline),
                        typeof(QueryReplyValidationPipeline),
                        typeof(HandledByEndpointPipeline),
                        typeof(MessageHandlerPipeline)
                    };

                    var actualPipeline = endpointDependencyContainer
                        .Resolve<IMessagePipeline>()
                        .FlattenDecoratedObject(obj => obj.GetType())
                        .ShowTypes("message pipeline", log)
                        .ToList();

                    Assert.Equal(expectedPipeline, actualPipeline);

                    var integrationTypeProvider = endpointDependencyContainer.Resolve<IIntegrationTypeProvider>();

                    var expectedIntegrationMessageTypes = new[]
                    {
                        typeof(CaptureDomainEvent<>),
                        typeof(BaseEvent),
                        typeof(InheritedEvent),
                        typeof(Command),
                        typeof(OpenGenericHandlerCommand),
                        typeof(Query),
                        typeof(Reply),
                        typeof(CaptureTrace),
                        typeof(GetConversationTrace),
                        typeof(ConversationTrace)
                    };

                    var actualIntegrationMessageTypes = integrationTypeProvider
                        .IntegrationMessageTypes()
                        .ShowTypes(nameof(IIntegrationTypeProvider.IntegrationMessageTypes), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedIntegrationMessageTypes.OrderBy(type => type.FullName).ToList(), actualIntegrationMessageTypes);

                    var expectedCommands = new[]
                    {
                        typeof(CaptureDomainEvent<>),
                        typeof(Command),
                        typeof(OpenGenericHandlerCommand)
                    };

                    var actualCommands = integrationTypeProvider
                        .EndpointCommands()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EndpointCommands), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedCommands.OrderBy(type => type.FullName).ToList(), actualCommands);

                    var expectedQueries = new[]
                    {
                        typeof(Query)
                    };

                    var actualQueries = integrationTypeProvider
                        .EndpointQueries()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EndpointQueries), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedQueries.OrderBy(type => type.FullName).ToList(), actualQueries);

                    var expectedReplies = new[]
                    {
                        typeof(Reply)
                    };

                    var actualReplies = integrationTypeProvider
                        .RepliesSubscriptions()
                        .ShowTypes(nameof(IIntegrationTypeProvider.RepliesSubscriptions), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedReplies.OrderBy(type => type.FullName).ToList(), actualReplies);

                    var expectedEvents = new[]
                    {
                        typeof(BaseEvent),
                        typeof(InheritedEvent)
                    };

                    var actualEvents = integrationTypeProvider
                        .EventsSubscriptions()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EventsSubscriptions), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedEvents.OrderBy(type => type.FullName).ToList(), actualEvents);

                    Assert.Equal(typeof(BaseEventEmptyMessageHandler), endpointDependencyContainer.Resolve<IMessageHandler<BaseEvent>>().GetType());
                    Assert.Equal(typeof(InheritedEventEmptyMessageHandler), endpointDependencyContainer.Resolve<IMessageHandler<InheritedEvent>>().GetType());
                    Assert.Equal(typeof(CommandEmptyMessageHandler), endpointDependencyContainer.Resolve<IMessageHandler<Command>>().GetType());
                    Assert.Equal(typeof(OpenGenericCommandEmptyMessageHandler<OpenGenericHandlerCommand>), endpointDependencyContainer.Resolve<IMessageHandler<OpenGenericHandlerCommand>>().GetType());
                    Assert.Equal(typeof(QueryAlwaysReplyMessageHandler), endpointDependencyContainer.Resolve<IMessageHandler<Query>>().GetType());
                    Assert.Equal(typeof(ReplyEmptyMessageHandler), endpointDependencyContainer.Resolve<IMessageHandler<Reply>>().GetType());

                    var expectedErrorHandlers = new[]
                    {
                        typeof(TracingErrorHandler),
                        typeof(RetryErrorHandler)
                    };

                    var actualErrorHandlers = endpointDependencyContainer
                       .ResolveCollection<IErrorHandler>()
                       .Select(obj => obj.GetType())
                       .ShowTypes(nameof(IErrorHandler), log)
                       .ToList();

                    Assert.Equal(expectedErrorHandlers, actualErrorHandlers);
                }
            }

            static void CheckTransport(IHost host, Action<string> log)
            {
                var transportDependencyContainer = host.GetTransportDependencyContainer();

                var integrationTransport = transportDependencyContainer.Resolve<IIntegrationTransport>();
                var endpointIdentity = transportDependencyContainer.Resolve<EndpointIdentity>();
                log($"Endpoint: {endpointIdentity}");
                log($"{nameof(IIntegrationTransport)}: {integrationTransport.GetType().FullName}");

                var integrationMessage = new IntegrationMessage(new Command(0), typeof(Command));

                Assert.Throws<ComponentResolutionException>(() => transportDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                Assert.Throws<ComponentResolutionException>(() => transportDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage));
                Assert.Throws<ComponentResolutionException>(() => transportDependencyContainer.Resolve<IIntegrationContext>());

                using (transportDependencyContainer.OpenScope())
                {
                    var expectedContexts = new[]
                    {
                        typeof(AdvancedIntegrationContext)
                    };

                    Assert.Throws<ComponentResolutionException>(() => transportDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                    var advancedIntegrationContext = transportDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage);

                    var actualAdvancedIntegrationContexts = advancedIntegrationContext
                        .FlattenDecoratedObject(obj => obj.GetType())
                        .ShowTypes("advanced integration context", log)
                        .ToList();

                    Assert.Equal(expectedContexts, actualAdvancedIntegrationContexts);

                    var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                    var actualIntegrationContexts = integrationContext
                        .FlattenDecoratedObject(obj => obj.GetType())
                        .ShowTypes("integration context", log)
                        .ToList();

                    Assert.Equal(expectedContexts, actualIntegrationContexts);

                    var expectedMessagesCollector = new[]
                    {
                        typeof(IntegrationTransportMessagesCollector)
                    };

                    var messagesCollector = transportDependencyContainer.Resolve<IMessagesCollector>();

                    var actualMessagesCollector = messagesCollector
                        .FlattenDecoratedObject(obj => obj.GetType())
                        .ShowTypes("collector", log)
                        .ToList();

                    Assert.Equal(expectedMessagesCollector, actualMessagesCollector);

                    var expectedPipeline = new[]
                    {
                        typeof(ErrorHandlingPipeline),
                        typeof(TracingPipeline),
                        typeof(UnitOfWorkPipeline),
                        typeof(QueryReplyValidationPipeline),
                        typeof(HandledByEndpointPipeline),
                        typeof(MessageHandlerPipeline)
                    };

                    var actualPipeline = transportDependencyContainer
                        .Resolve<IMessagePipeline>()
                        .FlattenDecoratedObject(obj => obj.GetType())
                        .ShowTypes("message pipeline", log)
                        .ToList();

                    Assert.Equal(expectedPipeline, actualPipeline);

                    var expectedIntegrationTypeProviders = new[]
                    {
                        typeof(IntegrationTransportIntegrationTypeProvider)
                    };

                    var integrationTypeProvider = transportDependencyContainer.Resolve<IIntegrationTypeProvider>();

                    var actualIntegrationTypeProviders = integrationTypeProvider
                       .FlattenDecoratedObject(obj => obj.GetType())
                       .ShowTypes("integration type provider", log)
                       .ToList();

                    Assert.Equal(expectedIntegrationTypeProviders, actualIntegrationTypeProviders);

                    var expectedIntegrationMessageTypes = new[]
                    {
                        typeof(CaptureDomainEvent<>),
                        typeof(AuthorizeUser),
                        typeof(UserAuthorizationResult),
                        typeof(CreateUser),
                        typeof(UserCreated),
                        typeof(BaseEvent),
                        typeof(InheritedEvent),
                        typeof(Event),
                        typeof(PublishEventCommand),
                        typeof(PublishInheritedEventCommand),
                        typeof(Command),
                        typeof(OpenGenericHandlerCommand),
                        typeof(Query),
                        typeof(Reply),
                        typeof(RequestQueryCommand),
                        typeof(CaptureTrace),
                        typeof(GetConversationTrace),
                        typeof(ConversationTrace),
                        typeof(Endpoint1HandlerInvoked),
                        typeof(Endpoint2HandlerInvoked)
                    };

                    var actualIntegrationMessageTypes = integrationTypeProvider
                        .IntegrationMessageTypes()
                        .ShowTypes(nameof(IIntegrationTypeProvider.IntegrationMessageTypes), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedIntegrationMessageTypes.OrderBy(type => type.FullName).ToList(), actualIntegrationMessageTypes);

                    var actualCommands = integrationTypeProvider
                        .EndpointCommands()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EndpointCommands), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(Array.Empty<Type>(), actualCommands);

                    var actualQueries = integrationTypeProvider
                        .EndpointQueries()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EndpointQueries), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(Array.Empty<Type>(), actualQueries);

                    var expectedReplies = new[]
                    {
                        typeof(UserAuthorizationResult),
                        typeof(Reply),
                        typeof(ConversationTrace)
                    };

                    var actualReplies = integrationTypeProvider
                        .RepliesSubscriptions()
                        .ShowTypes(nameof(IIntegrationTypeProvider.RepliesSubscriptions), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedReplies.OrderBy(type => type.FullName).ToList(), actualReplies);

                    var actualEvents = integrationTypeProvider
                        .EventsSubscriptions()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EventsSubscriptions), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(Array.Empty<Type>(), actualEvents);

                    var expectedErrorHandlers = new[]
                    {
                        typeof(TracingErrorHandler),
                        typeof(RetryErrorHandler)
                    };

                    var actualErrorHandlers = transportDependencyContainer
                       .ResolveCollection<IErrorHandler>()
                       .Select(obj => obj.GetType())
                       .ShowTypes(nameof(IErrorHandler), log)
                       .ToList();

                    Assert.Equal(expectedErrorHandlers, actualErrorHandlers);
                }

                _ = transportDependencyContainer.Resolve<IRpcRequestRegistry>();

                using (transportDependencyContainer.OpenScope())
                {
                    var expectedRpcReplyHandlers = new[]
                    {
                        typeof(RpcReplyMessageHandler<IIntegrationReply>)
                    };

                    var actualRpcReplyHandlers = transportDependencyContainer
                        .Resolve<IMessageHandler<IIntegrationReply>>()
                        .FlattenDecoratedObject(obj => obj.GetType())
                        .ShowTypes("rpc reply handlers", log)
                        .ToList();

                    Assert.Equal(expectedRpcReplyHandlers, actualRpcReplyHandlers);
                }
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostWithDataAccessTestData))]
        internal async Task CompareEquivalentDatabaseDatabaseModelsTest(
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            IDatabaseProvider databaseProvider)
        {
            Output.WriteLine(databaseProvider.GetType().FullName);

            var logger = Fixture.CreateLogger(Output);

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(nameof(CompareEquivalentDatabaseDatabaseModelsTest))
            };

            var host = useTransport(nameof(CompareEquivalentDatabaseDatabaseModelsTest), logger, Fixture.CreateHostBuilder(Output))
               .UseTracingEndpoint(TestIdentity.Instance0,
                    builder => builder
                       .WithDataAccess(databaseProvider)
                       .ModifyContainerOptions(options => options
                           .WithOverrides(overrides))
                       .BuildOptions())
               .ExecuteMigrations(builder => builder
                   .WithDataAccess(databaseProvider)
                   .ModifyContainerOptions(options => options
                       .WithOverrides(overrides))
                   .BuildOptions())
               .BuildHost();

            var migrationsContainer = host.GetMigrationsDependencyContainer();

            var actualModel = await migrationsContainer
                .Resolve<IDatabaseModelBuilder>()
                .BuildModel(CancellationToken.None)
                .ConfigureAwait(false);

            var databaseEntities = migrationsContainer
               .Resolve<IDatabaseTypeProvider>()
               .DatabaseEntities()
               .ToArray();

            var expectedModel = await migrationsContainer
                .Resolve<ICodeModelBuilder>()
                .BuildModel(databaseEntities, CancellationToken.None)
                .ConfigureAwait(false);

            var modelChanges = migrationsContainer
                .Resolve<IModelComparator>()
                .ExtractDiff(actualModel, expectedModel);

            Assert.NotEmpty(modelChanges);

            modelChanges = migrationsContainer
                .Resolve<IModelComparator>()
                .ExtractDiff(expectedModel, expectedModel);

            Assert.Empty(modelChanges);
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostWithDataAccessTestData))]
        internal async Task ExtractDatabaseModelChangesDiffTest(
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            IDatabaseProvider databaseProvider)
        {
            Output.WriteLine(databaseProvider.GetType().FullName);

            var logger = Fixture.CreateLogger(Output);

            var additionalOurTypes = new[]
            {
                typeof(Community),
                typeof(Participant),
                typeof(Blog),
                typeof(Post),
                typeof(User)
            };

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(nameof(ExtractDatabaseModelChangesDiffTest))
            };

            var host = useTransport(nameof(ExtractDatabaseModelChangesDiffTest), logger, Fixture.CreateHostBuilder(Output))
               .UseTracingEndpoint(TestIdentity.Instance0,
                    builder => builder
                       .WithDataAccess(databaseProvider)
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithOverrides(overrides))
                       .BuildOptions())
               .ExecuteMigrations(builder => builder
                   .WithDataAccess(databaseProvider)
                   .ModifyContainerOptions(options => options
                       .WithOverrides(overrides))
                   .BuildOptions())
               .BuildHost();

            var tracingEndpointContainer = host.GetEndpointDependencyContainer(
                new EndpointIdentity(TracingEndpointIdentity.LogicalName, TestIdentity.Instance0));

            var migrationsContainer = host.GetMigrationsDependencyContainer();

            var actualModel = await migrationsContainer
                .Resolve<IDatabaseModelBuilder>()
                .BuildModel(CancellationToken.None)
                .ConfigureAwait(false);

            var databaseEntities = migrationsContainer
               .Resolve<IDatabaseTypeProvider>()
               .DatabaseEntities()
               .ToArray();

            var expectedModel = await migrationsContainer
                .Resolve<ICodeModelBuilder>()
                .BuildModel(databaseEntities, CancellationToken.None)
                .ConfigureAwait(false);

            var unorderedModelChanges = migrationsContainer
                .Resolve<IModelComparator>()
                .ExtractDiff(actualModel, expectedModel);

            var modelChanges = migrationsContainer
                .Resolve<IModelChangesSorter>()
                .Sort(unorderedModelChanges)
                .ToArray();

            modelChanges.Each((change, i) => Output.WriteLine($"[{i}] {change}"));

            if (databaseProvider.GetType() == typeof(PostgreSqlDatabaseProvider))
            {
                var assertions = new Action<int>[]
                {
                    index => AssertCreateDataBase(modelChanges, index, nameof(ExtractDatabaseModelChangesDiffTest)),
                    index => AssertCreateSchema(modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication)),
                    index => AssertCreateSchema(modelChanges, index, nameof(GenericEndpoint.DataAccess.EventSourcing)),
                    index => AssertCreateSchema(modelChanges, index, nameof(GenericHost) + nameof(GenericHost.Test)),
                    index => AssertCreateSchema(modelChanges, index, nameof(DataAccess.Orm.Host.Migrations)),
                    index => AssertCreateSchema(modelChanges, index, nameof(GenericEndpoint.Tracing)),
                    index =>
                    {
                        AssertCreateTable(modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication), typeof(IntegrationMessageHeader));
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(IntegrationMessageHeader.PrimaryKey), "not null primary key");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(IntegrationMessageHeader.Version), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(IntegrationMessageHeader.Value)}_{nameof(JsonObject.SystemType)}_{nameof(SystemType.Assembly)}", "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(IntegrationMessageHeader.Value)}_{nameof(JsonObject.SystemType)}_{nameof(SystemType.Type)}", "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(IntegrationMessageHeader.Value)}_{nameof(JsonObject.Value)}", "not null");
                    },
                    index =>
                    {
                        AssertCreateTable(modelChanges, index, nameof(GenericEndpoint.DataAccess.EventSourcing), typeof(DatabaseDomainEvent));
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(DatabaseDomainEvent.PrimaryKey), "not null primary key");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(DatabaseDomainEvent.Version), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(DatabaseDomainEvent.AggregateId), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(DatabaseDomainEvent.Index), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(DatabaseDomainEvent.Timestamp), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(DatabaseDomainEvent.EventType)}_{nameof(SystemType.Assembly)}", "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(DatabaseDomainEvent.EventType)}_{nameof(SystemType.Type)}", "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(DatabaseDomainEvent.SerializedEvent), "not null");
                    },
                    index =>
                    {
                        AssertCreateTable(modelChanges, index, nameof(GenericHost) + nameof(GenericHost.Test), typeof(Blog));
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Blog.PrimaryKey), "not null primary key");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Blog.Version), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Blog.Theme), "not null");
                        AssertMtmColumn(tracingEndpointContainer, modelChanges, index, $"{nameof(Blog.Posts)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}");
                    },
                    index =>
                    {
                        AssertCreateTable(modelChanges, index, nameof(GenericHost) + nameof(GenericHost.Test), typeof(Community));
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Community.PrimaryKey), "not null primary key");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Community.Version), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Community.Name), "not null");
                        AssertMtmColumn(tracingEndpointContainer, modelChanges, index, $"{nameof(Community.Participants)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}");
                    },
                    index =>
                    {
                        AssertCreateTable(modelChanges, index, nameof(GenericHost) + nameof(GenericHost.Test), typeof(Participant));
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Participant.PrimaryKey), "not null primary key");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Participant.Version), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Participant.Name), "not null");
                        AssertMtmColumn(tracingEndpointContainer, modelChanges, index, $"{nameof(Participant.Communities)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right)}");
                    },
                    index =>
                    {
                        AssertCreateTable(modelChanges, index, nameof(GenericHost) + nameof(GenericHost.Test), typeof(User));
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(User.PrimaryKey), "not null primary key");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(User.Version), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(User.Nickname), "not null");
                    },
                    index =>
                    {
                        AssertCreateTable(modelChanges, index, nameof(DataAccess.Orm.Host.Migrations), typeof(AppliedMigration));
                        AssertColumnConstraints(migrationsContainer, modelChanges, index, nameof(AppliedMigration.PrimaryKey), "not null primary key");
                        AssertColumnConstraints(migrationsContainer, modelChanges, index, nameof(AppliedMigration.Version), "not null");
                        AssertColumnConstraints(migrationsContainer, modelChanges, index, nameof(AppliedMigration.DateTime), "not null");
                        AssertColumnConstraints(migrationsContainer, modelChanges, index, nameof(AppliedMigration.CommandText), "not null");
                        AssertColumnConstraints(migrationsContainer, modelChanges, index, nameof(AppliedMigration.Name), "not null");
                    },
                    index =>
                    {
                        AssertCreateTable(modelChanges, index, nameof(GenericEndpoint.Tracing), typeof(TracingEndpoint.DatabaseModel.IntegrationMessage));
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.PrimaryKey), "not null primary key");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.Version), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.MessageId), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.ConversationId), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.InitiatorMessageId), string.Empty);
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.Payload)}_{nameof(JsonObject.SystemType)}_{nameof(SystemType.Assembly)}", "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.Payload)}_{nameof(JsonObject.SystemType)}_{nameof(SystemType.Type)}", "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.Payload)}_{nameof(JsonObject.Value)}", "not null");
                    },
                    index =>
                    {
                        AssertCreateTable(modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication), typeof(GenericEndpoint.DataAccess.Deduplication.IntegrationMessage));
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication.IntegrationMessage.PrimaryKey), "not null primary key");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication.IntegrationMessage.Version), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(GenericEndpoint.DataAccess.Deduplication.IntegrationMessage.Payload)}_{nameof(JsonObject.SystemType)}_{nameof(SystemType.Assembly)}", "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(GenericEndpoint.DataAccess.Deduplication.IntegrationMessage.Payload)}_{nameof(JsonObject.SystemType)}_{nameof(SystemType.Type)}", "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(GenericEndpoint.DataAccess.Deduplication.IntegrationMessage.Payload)}_{nameof(JsonObject.Value)}", "not null");
                        AssertMtmColumn(tracingEndpointContainer, modelChanges, index, $"{nameof(GenericEndpoint.DataAccess.Deduplication.IntegrationMessage.Headers)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}");
                    },
                    index =>
                    {
                        AssertCreateMtmTable(modelChanges, index, nameof(GenericHost) + nameof(GenericHost.Test), $"{nameof(Community)}_{nameof(Participant)}");
                        AssertHasNoColumn(tracingEndpointContainer, modelChanges, index, nameof(IUniqueIdentified.PrimaryKey));
                        AssertHasNoColumn(tracingEndpointContainer, modelChanges, index, nameof(IDatabaseEntity.Version));
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), $@"not null references ""{nameof(GenericHost) + nameof(GenericHost.Test)}"".""{nameof(Community)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), $@"not null references ""{nameof(GenericHost) + nameof(GenericHost.Test)}"".""{nameof(Participant)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                    },
                    index =>
                    {
                        AssertCreateTable(modelChanges, index, nameof(GenericHost) + nameof(GenericHost.Test), typeof(Post));
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Post.PrimaryKey), "not null primary key");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Post.Version), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Post.DateTime), "not null");
                        AssertHasNoColumn(tracingEndpointContainer, modelChanges, index, $"{nameof(Blog)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}");
                        AssertHasNoColumn(tracingEndpointContainer, modelChanges, index, $"{nameof(Blog)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right)}");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}", $@"not null references ""{nameof(GenericHost) + nameof(GenericHost.Test)}"".""{nameof(Blog)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                    },
                    index =>
                    {
                        AssertCreateTable(modelChanges, index, nameof(GenericEndpoint.Tracing), typeof(CapturedMessage));
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(CapturedMessage.PrimaryKey), "not null primary key");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(CapturedMessage.Version), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(CapturedMessage.Message)}_{nameof(CapturedMessage.Message.PrimaryKey)}", $@"not null references ""{nameof(GenericEndpoint.Tracing)}"".""{nameof(TracingEndpoint.DatabaseModel.IntegrationMessage)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(CapturedMessage.RefuseReason), string.Empty);
                    },
                    index =>
                    {
                        AssertCreateTable(modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication), typeof(InboxMessage));
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(InboxMessage.PrimaryKey), "not null primary key");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(InboxMessage.Version), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(InboxMessage.Message)}_{nameof(InboxMessage.Message.PrimaryKey)}", $@"not null references ""{nameof(GenericEndpoint.DataAccess.Deduplication)}"".""{nameof(IntegrationMessage)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(InboxMessage.EndpointIdentity)}_{nameof(GenericEndpoint.DataAccess.Deduplication.EndpointIdentity.LogicalName)}", "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(InboxMessage.EndpointIdentity)}_{nameof(GenericEndpoint.DataAccess.Deduplication.EndpointIdentity.InstanceName)}", "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(InboxMessage.IsError), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(InboxMessage.Handled), "not null");
                    },
                    index =>
                    {
                        AssertCreateMtmTable(modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication), $"{nameof(IntegrationMessage)}_{nameof(IntegrationMessageHeader)}");
                        AssertHasNoColumn(tracingEndpointContainer, modelChanges, index, nameof(IUniqueIdentified.PrimaryKey));
                        AssertHasNoColumn(tracingEndpointContainer, modelChanges, index, nameof(IDatabaseEntity.Version));
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), $@"not null references ""{nameof(GenericEndpoint.DataAccess.Deduplication)}"".""{nameof(IntegrationMessage)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), $@"not null references ""{nameof(GenericEndpoint.DataAccess.Deduplication)}"".""{nameof(IntegrationMessageHeader)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                    },
                    index =>
                    {
                        AssertCreateTable(modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication), typeof(OutboxMessage));
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(OutboxMessage.PrimaryKey), "not null primary key");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(OutboxMessage.Version), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(OutboxMessage.OutboxId), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(OutboxMessage.Timestamp), "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(OutboxMessage.EndpointIdentity)}_{nameof(GenericEndpoint.DataAccess.Deduplication.EndpointIdentity.LogicalName)}", "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(OutboxMessage.EndpointIdentity)}_{nameof(GenericEndpoint.DataAccess.Deduplication.EndpointIdentity.InstanceName)}", "not null");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(OutboxMessage.Message)}_{nameof(OutboxMessage.Message.PrimaryKey)}", $@"not null references ""{nameof(GenericEndpoint.DataAccess.Deduplication)}"".""{nameof(IntegrationMessage)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(OutboxMessage.Sent), "not null");
                    },
                    index =>
                    {
                        AssertCreateMtmTable(modelChanges, index, nameof(GenericHost) + nameof(GenericHost.Test), $"{nameof(Blog)}_{nameof(Post)}");
                        AssertHasNoColumn(tracingEndpointContainer, modelChanges, index, nameof(IUniqueIdentified.PrimaryKey));
                        AssertHasNoColumn(tracingEndpointContainer, modelChanges, index, nameof(IDatabaseEntity.Version));
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), $@"not null references ""{nameof(GenericHost) + nameof(GenericHost.Test)}"".""{nameof(Blog)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                        AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), $@"not null references ""{nameof(GenericHost) + nameof(GenericHost.Test)}"".""{nameof(Post)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                    },
                    index => AssertCreateView(modelChanges, index, nameof(DatabaseColumn)),
                    index => AssertCreateView(modelChanges, index, nameof(DatabaseColumnConstraint)),
                    index => AssertCreateView(modelChanges, index, nameof(DatabaseIndex)),
                    index => AssertCreateView(modelChanges, index, nameof(DatabaseSchema)),
                    index => AssertCreateView(modelChanges, index, nameof(DatabaseView)),
                    index => AssertCreateIndex(modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication), $"{nameof(IntegrationMessage)}_{nameof(IntegrationMessageHeader)}", $"{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right)}"),
                    index => AssertCreateIndex(modelChanges, index, nameof(GenericEndpoint.DataAccess.EventSourcing), nameof(DatabaseDomainEvent), $"{nameof(DatabaseDomainEvent.AggregateId)}_{nameof(DatabaseDomainEvent.Index)}"),
                    index => AssertCreateIndex(modelChanges, index, nameof(GenericHost) + nameof(GenericHost.Test), $"{nameof(Blog)}_{nameof(Post)}", $"{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right)}"),
                    index => AssertCreateIndex(modelChanges, index, nameof(GenericHost) + nameof(GenericHost.Test), $"{nameof(Community)}_{nameof(Participant)}", $"{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right)}"),
                    index => AssertCreateIndex(modelChanges, index, nameof(DataAccess.Orm.Host.Migrations), nameof(AppliedMigration), nameof(AppliedMigration.Name)),
                    index => AssertCreateIndex(modelChanges, index, nameof(DataAccess.Orm.Host.Migrations), nameof(DatabaseColumn), $"{nameof(DatabaseColumn.Column)}_{nameof(DatabaseColumn.Schema)}_{nameof(DatabaseColumn.Table)}"),
                    index => AssertCreateIndex(modelChanges, index, nameof(DataAccess.Orm.Host.Migrations), nameof(DatabaseIndex), $"{nameof(DatabaseIndex.Index)}_{nameof(DatabaseIndex.Schema)}_{nameof(DatabaseIndex.Table)}"),
                    index => AssertCreateIndex(modelChanges, index, nameof(DataAccess.Orm.Host.Migrations), nameof(DatabaseSchema), $"{nameof(DatabaseSchema.Name)}"),
                    index => AssertCreateIndex(modelChanges, index, nameof(DataAccess.Orm.Host.Migrations), nameof(DatabaseView), $"{nameof(DatabaseView.Query)}_{nameof(DatabaseView.Schema)}_{nameof(DatabaseView.View)}")
                };

                Assert.Equal(assertions.Length, modelChanges.Length);

                for (var i = 0; i < assertions.Length; i++)
                {
                    assertions[i](i);
                }

                static void AssertCreateTable(IModelChange[] modelChanges, int index, string schema, Type table)
                {
                    Assert.True(modelChanges[index] is CreateTable);
                    var createTable = (CreateTable)modelChanges[index];
                    Assert.Equal($"{schema}.{table.Name}", $"{createTable.Schema}.{createTable.Table}");
                }

                static void AssertCreateMtmTable(IModelChange[] modelChanges, int index, string schema, string table)
                {
                    Assert.True(modelChanges[index] is CreateTable);
                    var createTable = (CreateTable)modelChanges[index];
                    Assert.Equal($"{schema}.{table}", $"{createTable.Schema}.{createTable.Table}");
                }

                static void AssertColumnConstraints(IDependencyContainer dependencyContainer, IModelChange[] modelChanges, int index, string column, string constraints)
                {
                    Assert.True(modelChanges[index] is CreateTable);
                    var createTable = (CreateTable)modelChanges[index];
                    var modelProvider = dependencyContainer.Resolve<IModelProvider>();
                    Assert.True(modelProvider.TablesMap.ContainsKey(createTable.Schema));
                    Assert.True(modelProvider.TablesMap[createTable.Schema].ContainsKey(createTable.Table));
                    Assert.True(modelProvider.TablesMap[createTable.Schema][createTable.Table] is TableInfo);
                    var tableInfo = (TableInfo)modelProvider.TablesMap[createTable.Schema][createTable.Table];
                    Assert.True(tableInfo.Columns.ContainsKey(column));
                    var columnInfo = tableInfo.Columns[column];
                    var actualConstraints = columnInfo.Constraints.ToString(" ");
                    Assert.True(actualConstraints.Equals(constraints, StringComparison.OrdinalIgnoreCase));
                    Assert.False(columnInfo.IsMultipleRelation);

                    if (constraints.Contains("references", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.NotNull(columnInfo.Relation);
                    }
                    else
                    {
                        Assert.Null(columnInfo.Relation);
                    }
                }

                static void AssertMtmColumn(IDependencyContainer dependencyContainer, IModelChange[] modelChanges, int index, string column)
                {
                    Assert.True(modelChanges[index] is CreateTable);
                    var createTable = (CreateTable)modelChanges[index];
                    var modelProvider = dependencyContainer.Resolve<IModelProvider>();
                    Assert.True(modelProvider.TablesMap.ContainsKey(createTable.Schema));
                    Assert.True(modelProvider.TablesMap[createTable.Schema].ContainsKey(createTable.Table));
                    Assert.True(modelProvider.TablesMap[createTable.Schema][createTable.Table] is TableInfo);
                    var tableInfo = (TableInfo)modelProvider.TablesMap[createTable.Schema][createTable.Table];
                    Assert.True(tableInfo.Columns.ContainsKey(column));
                    var columnInfo = tableInfo.Columns[column];
                    Assert.True(columnInfo.IsMultipleRelation);
                    Assert.NotNull(columnInfo.Relation);
                }

                static void AssertHasNoColumn(IDependencyContainer dependencyContainer, IModelChange[] modelChanges, int index, string column)
                {
                    Assert.True(modelChanges[index] is CreateTable);
                    var createTable = (CreateTable)modelChanges[index];
                    var modelProvider = dependencyContainer.Resolve<IModelProvider>();
                    Assert.True(modelProvider.TablesMap.ContainsKey(createTable.Schema));
                    Assert.True(modelProvider.TablesMap[createTable.Schema].ContainsKey(createTable.Table));
                    Assert.True(modelProvider.TablesMap[createTable.Schema][createTable.Table] is TableInfo);
                    var tableInfo = (TableInfo)modelProvider.TablesMap[createTable.Schema][createTable.Table];
                    Assert.True(tableInfo.Columns.Keys.All(key => !key.Contains(column, StringComparison.OrdinalIgnoreCase)));
                }
            }
            else
            {
                throw new NotSupportedException(databaseProvider.GetType().FullName);
            }

            static void AssertCreateDataBase(IModelChange[] modelChanges, int index, string database)
            {
                Assert.True(modelChanges[index] is CreateDatabase);
                var createDatabase = (CreateDatabase)modelChanges[index];
                Assert.True(createDatabase.Database.Equals(database, StringComparison.OrdinalIgnoreCase));
            }

            static void AssertCreateSchema(IModelChange[] modelChanges, int index, string schema)
            {
                Assert.True(modelChanges[index] is CreateSchema);
                var createSchema = (CreateSchema)modelChanges[index];
                Assert.True(createSchema.Schema.Equals(schema, StringComparison.OrdinalIgnoreCase));
            }

            static void AssertCreateView(IModelChange[] modelChanges, int index, string view)
            {
                Assert.True(modelChanges[index] is CreateView);
                var createView = (CreateView)modelChanges[index];
                Assert.True(createView.View.Equals(view, StringComparison.OrdinalIgnoreCase));
            }

            static void AssertCreateIndex(IModelChange[] modelChanges, int index, string schema, string table, string indexName)
            {
                Assert.True(modelChanges[index] is CreateIndex);
                var createIndex = (CreateIndex)modelChanges[index];
                Assert.True(createIndex.Schema.Equals(schema, StringComparison.OrdinalIgnoreCase));
                Assert.True(createIndex.Table.Equals(table, StringComparison.OrdinalIgnoreCase));
                Assert.True(createIndex.Index.Equals(string.Join("__", table, indexName), StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}