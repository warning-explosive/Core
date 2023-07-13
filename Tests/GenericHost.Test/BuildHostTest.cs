namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AuthEndpoint.Contract;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Exceptions;
    using CompositionRoot.Extensions;
    using DataAccess.Orm.PostgreSql.Host.StartupActions;
    using DataAccess.Orm.Sql.Connection;
    using DataAccess.Orm.Sql.Host.Model;
    using DataAccess.Orm.Sql.Model;
    using DataAccess.Orm.Sql.Model.Attributes;
    using DataAccess.Orm.Sql.Transaction;
    using DatabaseEntities;
    using DatabaseEntities.Relations;
    using GenericDomain.EventSourcing.Sql;
    using GenericEndpoint.Authorization;
    using GenericEndpoint.Authorization.Host;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.DataAccess.Sql.Deduplication;
    using GenericEndpoint.DataAccess.Sql.Host;
    using GenericEndpoint.DataAccess.Sql.Host.BackgroundWorkers;
    using GenericEndpoint.DataAccess.Sql.Host.StartupActions;
    using GenericEndpoint.EventSourcing.Host;
    using GenericEndpoint.EventSourcing.Host.StartupActions;
    using GenericEndpoint.Host;
    using GenericEndpoint.Host.Builder;
    using GenericEndpoint.Host.StartupActions;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Pipeline;
    using GenericEndpoint.RpcRequest;
    using GenericHost;
    using IntegrationTransport.Host;
    using IntegrationTransport.Host.BackgroundWorkers;
    using IntegrationTransport.Integration;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using SpaceEngineers.Core.GenericEndpoint.Api.Abstractions;
    using SpaceEngineers.Core.IntegrationTransport.Api.Abstractions;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
    using StartupActions;
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
        /// <param name="fixture">TestFixture</param>
        public BuildHostTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// Test cases for BuildHostTest
        /// </summary>
        /// <returns>Test cases</returns>
        public static IEnumerable<object[]> BuildHostTestData()
        {
            var projectFileDirectory = SolutionExtensions.ProjectFile().Directory
                                       ?? throw new InvalidOperationException("Project directory wasn't found");

            var settingsDirectory = projectFileDirectory
                .StepInto("Settings")
                .StepInto(nameof(BuildHostTest));

            var useInMemoryIntegrationTransport = new Func<IHostBuilder, IHostBuilder>(
                hostBuilder => hostBuilder
                   .UseIntegrationTransport((_, builder) => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .BuildOptions()));

            var useRabbitMqIntegrationTransport = new Func<IHostBuilder, IHostBuilder>(
                hostBuilder => hostBuilder
                   .UseIntegrationTransport((_, builder) => builder
                       .WithRabbitMqIntegrationTransport(hostBuilder)
                       .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                useInMemoryIntegrationTransport,
                useRabbitMqIntegrationTransport
            };

            return integrationTransportProviders
               .Select(useTransport => new object[]
               {
                   settingsDirectory,
                   useTransport
               });
        }

        /// <summary>
        /// Test cases for BuildHostTest
        /// </summary>
        /// <returns>Test cases</returns>
        public static IEnumerable<object[]> BuildHostWithDataAccessTestData()
        {
            var timeout = TimeSpan.FromSeconds(60);

            var projectFileDirectory = SolutionExtensions.ProjectFile().Directory
                                       ?? throw new InvalidOperationException("Project directory wasn't found");

            var settingsDirectory = projectFileDirectory
                .StepInto("Settings")
                .StepInto(nameof(BuildHostTest));

            var useInMemoryIntegrationTransport = new Func<IHostBuilder, IHostBuilder>(
                static hostBuilder => hostBuilder
                   .UseIntegrationTransport((_, builder) => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .BuildOptions()));

            var useRabbitMqIntegrationTransport = new Func<IHostBuilder, IHostBuilder>(
                static hostBuilder => hostBuilder
                   .UseIntegrationTransport((_, builder) => builder
                       .WithRabbitMqIntegrationTransport(hostBuilder)
                       .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                useInMemoryIntegrationTransport,
                useRabbitMqIntegrationTransport
            };

            var dataAccessProviders = new Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder>[]
            {
                (builder, dataAccessOptions) => builder.WithPostgreSqlDataAccess(dataAccessOptions)
            };

            var eventSourcingProviders = new Func<IEndpointBuilder, IEndpointBuilder>[]
            {
                builder => builder.WithSqlEventSourcing()
            };

            return integrationTransportProviders
               .SelectMany(useTransport => dataAccessProviders
                   .SelectMany(withDataAccess => eventSourcingProviders
                       .Select(withEventSourcing => new object[]
                       {
                           settingsDirectory,
                           useTransport,
                           withDataAccess,
                           withEventSourcing,
                           timeout
                       })));
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostTestData))]
        internal void SameTransportTest(
            DirectoryInfo settingsDirectory,
            Func<IHostBuilder, IHostBuilder> useTransport)
        {
            SameComponentTest(
                settingsDirectory,
                useTransport,
                dependencyContainer => dependencyContainer.Resolve<IIntegrationTransport>());
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostTestData))]
        internal void SameRpcRequestRegistryTest(
            DirectoryInfo settingsDirectory,
            Func<IHostBuilder, IHostBuilder> useTransport)
        {
            SameComponentTest(
                settingsDirectory,
                useTransport,
                dependencyContainer => dependencyContainer.Resolve<IRpcRequestRegistry>());
        }

        internal void SameComponentTest(
            DirectoryInfo settingsDirectory,
            Func<IHostBuilder, IHostBuilder> useTransport,
            Func<IDependencyContainer, object> resolve)
        {
            var host = useTransport(Fixture.CreateHostBuilder())
                .UseEndpoint(
                    TestIdentity.Endpoint10,
                    TestIdentity.Endpoint1Assembly,
                    (_, builder) => builder.BuildOptions())
                .UseEndpoint(
                    TestIdentity.Endpoint20,
                    TestIdentity.Endpoint2Assembly,
                    (_, builder) => builder.BuildOptions())
                .BuildHost(settingsDirectory);

            var gatewayHost = useTransport(Fixture.CreateHostBuilder()).BuildHost(settingsDirectory);

            var component = resolve(host.GetTransportDependencyContainer());
            var gatewayComponent = resolve(gatewayHost.GetTransportDependencyContainer());

            Assert.NotSame(component, gatewayComponent);
            Assert.Same(component, resolve(host.GetEndpointDependencyContainer(TestIdentity.Endpoint10)));
            Assert.Same(component, resolve(host.GetEndpointDependencyContainer(TestIdentity.Endpoint20)));
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostTestData))]
        internal void BuildTest(
            DirectoryInfo settingsDirectory,
            Func<IHostBuilder, IHostBuilder> useTransport)
        {
            var messageTypes = new[]
            {
                typeof(BaseEvent),
                typeof(InheritedEvent),
                typeof(Command),
                typeof(OpenGenericHandlerCommand),
                typeof(Request),
                typeof(Reply)
            };

            var messageHandlerTypes = new[]
            {
                typeof(BaseEventEmptyMessageHandler),
                typeof(InheritedEventEmptyMessageHandler),
                typeof(CommandEmptyMessageHandler),
                typeof(OpenGenericCommandEmptyMessageHandler<>),
                typeof(AlwaysReplyMessageHandler),
                typeof(ReplyEmptyMessageHandler)
            };

            var additionalOurTypes = messageTypes
               .Concat(messageHandlerTypes)
               .ToArray();

            var host = useTransport(Fixture.CreateHostBuilder())
                .UseEndpoint(TestIdentity.Endpoint10,
                    TestIdentity.Endpoint1Assembly,
                    (context, builder) => builder
                        .WithPostgreSqlDataAccess(options => options
                            .ExecuteMigrations())
                        .WithSqlEventSourcing()
                        .WithAuthorization(context.Configuration)
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(additionalOurTypes))
                        .BuildOptions())
                .BuildHost(settingsDirectory);

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
                        typeof(InboxInvalidationHostStartupAction),
                        typeof(UpgradeDatabaseHostStartupAction),
                        typeof(EventSourcingHostStartupAction),
                        typeof(ReloadNpgsqlTypesHostStartupAction),
                        typeof(GenericEndpointHostStartupAction),
                        typeof(GenericEndpointHostStartupAction),
                        typeof(MessagingHostStartupAction),
                        typeof(MessagingHostStartupAction)
                    }
                    .OrderByDependencies()
                    .ThenBy(type => type.Name)
                    .ToList();

                var actualHostStartupActions = host
                   .Services
                   .GetServices<IDependencyContainer>()
                   .SelectMany(dependencyContainer => dependencyContainer.ResolveCollection<IHostStartupAction>())
                   .Select(startup => startup.GetType())
                   .OrderByDependencies()
                   .ThenBy(type => type.Name)
                   .ToList();

                Assert.Equal(expectedHostStartupActions, actualHostStartupActions);

                var expectedHostBackgroundWorkers = new[]
                    {
                        typeof(GenericEndpointDataAccessHostBackgroundWorker),
                        typeof(IntegrationTransportHostBackgroundWorker)
                    }
                    .OrderByDependencies()
                    .ThenBy(type => type.Name)
                    .ToList();

                var actualHostBackgroundWorkers = host
                   .Services
                   .GetServices<IDependencyContainer>()
                   .SelectMany(dependencyContainer => dependencyContainer.ResolveCollection<IHostBackgroundWorker>())
                   .Select(startup => startup.GetType())
                   .OrderByDependencies()
                   .ThenBy(type => type.Name)
                   .ToList();

                Assert.Equal(expectedHostBackgroundWorkers, actualHostBackgroundWorkers);
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
                        typeof(TracingMiddleware),
                        typeof(ErrorHandlingMiddleware),
                        typeof(AuthorizationMiddleware),
                        typeof(UnitOfWorkMiddleware),
                        typeof(HandledByEndpointMiddleware),
                        typeof(RequestReplyMiddleware)
                    };

                    var actualPipeline = endpointDependencyContainer
                        .ResolveCollection<IMessageHandlerMiddleware>()
                        .Select(middleware => middleware.GetType())
                        .ShowTypes("message pipeline", log)
                        .ToList();

                    Assert.Equal(expectedPipeline, actualPipeline);

                    var integrationTypeProvider = endpointDependencyContainer.Resolve<IIntegrationTypeProvider>();

                    var expectedIntegrationMessageTypes = new[]
                        {
                            typeof(BaseEvent),
                            typeof(InheritedEvent),
                            typeof(Command),
                            typeof(OpenGenericHandlerCommand),
                            typeof(Request),
                            typeof(Reply)
                        }
                        .OrderBy(type => type.Name)
                        .ToList();

                    var actualIntegrationMessageTypes = integrationTypeProvider
                        .IntegrationMessageTypes()
                        .ShowTypes(nameof(IIntegrationTypeProvider.IntegrationMessageTypes), log)
                        .OrderBy(type => type.Name)
                        .ToList();

                    Assert.Equal(expectedIntegrationMessageTypes, actualIntegrationMessageTypes);

                    var expectedCommands = new[]
                        {
                            typeof(Command),
                            typeof(OpenGenericHandlerCommand)
                        }
                        .OrderBy(type => type.Name)
                        .ToList();

                    var actualCommands = integrationTypeProvider
                        .EndpointCommands()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EndpointCommands), log)
                        .OrderBy(type => type.Name)
                        .ToList();

                    Assert.Equal(expectedCommands, actualCommands);

                    var expectedRequests = new[]
                        {
                            typeof(Request)
                        }
                        .OrderBy(type => type.Name)
                        .ToList();

                    var actualRequests = integrationTypeProvider
                        .EndpointRequests()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EndpointRequests), log)
                        .OrderBy(type => type.Name)
                        .ToList();

                    Assert.Equal(expectedRequests, actualRequests);

                    var expectedReplies = new[]
                        {
                            typeof(Reply)
                        }
                        .OrderBy(type => type.Name)
                        .ToList();

                    var actualReplies = integrationTypeProvider
                        .RepliesSubscriptions()
                        .ShowTypes(nameof(IIntegrationTypeProvider.RepliesSubscriptions), log)
                        .OrderBy(type => type.Name)
                        .ToList();

                    Assert.Equal(expectedReplies, actualReplies);

                    var expectedEvents = new[]
                        {
                            typeof(BaseEvent),
                            typeof(InheritedEvent)
                        }
                        .OrderBy(type => type.Name)
                        .ToList();

                    var actualEvents = integrationTypeProvider
                        .EventsSubscriptions()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EventsSubscriptions), log)
                        .OrderBy(type => type.Name)
                        .ToList();

                    Assert.Equal(expectedEvents, actualEvents);

                    Assert.Equal(typeof(BaseEventEmptyMessageHandler), endpointDependencyContainer.Resolve<IMessageHandler<BaseEvent>>().GetType());
                    Assert.Equal(typeof(InheritedEventEmptyMessageHandler), endpointDependencyContainer.Resolve<IMessageHandler<InheritedEvent>>().GetType());
                    Assert.Equal(typeof(CommandEmptyMessageHandler), endpointDependencyContainer.Resolve<IMessageHandler<Command>>().GetType());
                    Assert.Equal(typeof(OpenGenericCommandEmptyMessageHandler<OpenGenericHandlerCommand>), endpointDependencyContainer.Resolve<IMessageHandler<OpenGenericHandlerCommand>>().GetType());
                    Assert.Equal(typeof(AlwaysReplyMessageHandler), endpointDependencyContainer.Resolve<IMessageHandler<Request>>().GetType());
                    Assert.Equal(typeof(ReplyEmptyMessageHandler), endpointDependencyContainer.Resolve<IMessageHandler<Reply>>().GetType());

                    var expectedErrorHandlers = new[]
                    {
                        typeof(RpcRequestErrorHandler),
                        typeof(RetryErrorHandler),
                        typeof(TracingErrorHandler)
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
                        typeof(TracingMiddleware),
                        typeof(ErrorHandlingMiddleware),
                        typeof(UnitOfWorkMiddleware),
                        typeof(HandledByEndpointMiddleware),
                        typeof(RequestReplyMiddleware)
                    };

                    var actualPipeline = transportDependencyContainer
                        .ResolveCollection<IMessageHandlerMiddleware>()
                        .Select(middleware => middleware.GetType())
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
                            typeof(AuthenticateUser),
                            typeof(UserAuthenticationResult),
                            typeof(CreateUser),
                            typeof(UserWasCreated),
                            typeof(BaseEvent),
                            typeof(InheritedEvent),
                            typeof(Event),
                            typeof(TransportEvent),
                            typeof(PublishEventCommand),
                            typeof(PublishInheritedEventCommand),
                            typeof(Command),
                            typeof(OpenGenericHandlerCommand),
                            typeof(Request),
                            typeof(Reply),
                            typeof(MakeRequestCommand),
                            typeof(Endpoint1HandlerInvoked),
                            typeof(Endpoint2HandlerInvoked)
                        }
                        .OrderBy(type => type.Name)
                        .ToList();

                    var actualIntegrationMessageTypes = integrationTypeProvider
                        .IntegrationMessageTypes()
                        .ShowTypes(nameof(IIntegrationTypeProvider.IntegrationMessageTypes), log)
                        .OrderBy(type => type.Name)
                        .ToList();

                    Assert.Equal(expectedIntegrationMessageTypes, actualIntegrationMessageTypes);

                    var actualCommands = integrationTypeProvider
                        .EndpointCommands()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EndpointCommands), log)
                        .OrderBy(type => type.Name)
                        .ToList();

                    Assert.Equal(Array.Empty<Type>(), actualCommands);

                    var actualRequests = integrationTypeProvider
                        .EndpointRequests()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EndpointRequests), log)
                        .OrderBy(type => type.Name)
                        .ToList();

                    Assert.Equal(Array.Empty<Type>(), actualRequests);

                    var expectedReplies = new[]
                        {
                            typeof(Reply),
                            typeof(UserAuthenticationResult)
                        }
                        .OrderBy(type => type.Name)
                        .ToList();

                    var actualReplies = integrationTypeProvider
                        .RepliesSubscriptions()
                        .ShowTypes(nameof(IIntegrationTypeProvider.RepliesSubscriptions), log)
                        .OrderBy(type => type.Name)
                        .ToList();

                    Assert.Equal(expectedReplies, actualReplies);

                    var actualEvents = integrationTypeProvider
                        .EventsSubscriptions()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EventsSubscriptions), log)
                        .OrderBy(type => type.Name)
                        .ToList();

                    Assert.Equal(Array.Empty<Type>(), actualEvents);

                    var expectedErrorHandlers = new[]
                    {
                        typeof(RpcRequestErrorHandler),
                        typeof(RetryErrorHandler),
                        typeof(TracingErrorHandler)
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

                var expectedUserScopeProviders = new[]
                {
                    typeof(TraceContextPropagationProvider),
                    typeof(ConversationIdProvider),
                    typeof(MessageInitiatorProvider),
                    typeof(MessageOriginProvider),
                    typeof(UserScopeProvider),
                    typeof(AnonymousUserScopeProvider)
                };

                var actualUserScopeProviders = transportDependencyContainer
                   .ResolveCollection<IIntegrationMessageHeaderProvider>()
                   .Select(obj => obj.GetType())
                   .ShowTypes(nameof(IIntegrationMessageHeaderProvider), log)
                   .ToList();

                Assert.Equal(expectedUserScopeProviders, actualUserScopeProviders);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostWithDataAccessTestData))]
        internal async Task CompareEquivalentDatabaseDatabaseModelsTest(
            DirectoryInfo settingsDirectory,
            Func<IHostBuilder, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            TimeSpan timeout)
        {
            var startupActions = new[]
            {
                typeof(RecreatePostgreSqlDatabaseHostStartupAction)
            };

            var host = useTransport(Fixture.CreateHostBuilder())
                .UseEndpoint(TestIdentity.Endpoint10,
                    TestIdentity.Endpoint1Assembly,
                    (_, builder) => withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(startupActions))
                        .BuildOptions())
                .BuildHost(settingsDirectory);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var endpointContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                await endpointContainer
                    .Resolve<RecreatePostgreSqlDatabaseHostStartupAction>()
                    .Run(cts.Token)
                    .ConfigureAwait(false);

                var actualModel = await endpointContainer.InvokeWithinTransaction(
                        false,
                        endpointContainer.Resolve<IDatabaseModelBuilder>().BuildModel,
                        cts.Token)
                    .ConfigureAwait(false);

                var databaseEntities = endpointContainer
                    .Resolve<IDatabaseTypeProvider>()
                    .DatabaseEntities()
                    .ToArray();

                var expectedModel = await endpointContainer
                    .Resolve<ICodeModelBuilder>()
                    .BuildModel(databaseEntities, cts.Token)
                    .ConfigureAwait(false);

                var modelChanges = endpointContainer
                    .Resolve<IModelComparator>()
                    .ExtractDiff(actualModel, expectedModel);

                Assert.NotEmpty(modelChanges);

                modelChanges = endpointContainer
                    .Resolve<IModelComparator>()
                    .ExtractDiff(expectedModel, expectedModel);

                Assert.Empty(modelChanges);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostWithDataAccessTestData))]
        internal async Task ExtractDatabaseModelChangesDiffTest(
            DirectoryInfo settingsDirectory,
            Func<IHostBuilder, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            TimeSpan timeout)
        {
            var databaseEntities = new[]
            {
                typeof(DatabaseEntity),
                typeof(ComplexDatabaseEntity),
                typeof(Community),
                typeof(Participant),
                typeof(Blog),
                typeof(Post),
                typeof(User)
            };

            var startupActions = new[]
            {
                typeof(RecreatePostgreSqlDatabaseHostStartupAction)
            };

            var additionalOurTypes = databaseEntities
                .Concat(startupActions)
                .ToArray();

            var host = useTransport(Fixture.CreateHostBuilder())
                .UseEndpoint(TestIdentity.Endpoint10,
                    TestIdentity.Endpoint1Assembly,
                    (_, builder) => withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(additionalOurTypes)
                            .WithAdditionalOurTypes(typeof(RecreatePostgreSqlDatabaseHostStartupAction)))
                        .BuildOptions())
                .BuildHost(settingsDirectory);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var endpointContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                await endpointContainer
                    .Resolve<RecreatePostgreSqlDatabaseHostStartupAction>()
                    .Run(cts.Token)
                    .ConfigureAwait(false);

                var modelProvider = endpointContainer.Resolve<IModelProvider>();

                var actualModel = await endpointContainer.InvokeWithinTransaction(
                        false,
                        endpointContainer.Resolve<IDatabaseModelBuilder>().BuildModel,
                        cts.Token)
                    .ConfigureAwait(false);

                databaseEntities = endpointContainer
                   .Resolve<IDatabaseTypeProvider>()
                   .DatabaseEntities()
                   .ToArray();

                var expectedModel = await endpointContainer
                    .Resolve<ICodeModelBuilder>()
                    .BuildModel(databaseEntities, cts.Token)
                    .ConfigureAwait(false);

                var unorderedModelChanges = endpointContainer
                    .Resolve<IModelComparator>()
                    .ExtractDiff(actualModel, expectedModel);

                var modelChanges = endpointContainer
                    .Resolve<IModelChangesSorter>()
                    .Sort(unorderedModelChanges)
                    .ToArray();

                modelChanges.Each((change, i) => Output.WriteLine($"[{i}] {change}"));

                var databaseConnectionProvider = endpointContainer.Resolve<IDatabaseConnectionProvider>();

                if (databaseConnectionProvider.GetType() == typeof(DataAccess.Orm.PostgreSql.Connection.DatabaseConnectionProvider))
                {
                    var assertions = new Action<int>[]
                    {
                        index => AssertCreateSchema(modelChanges, index, nameof(GenericEndpoint.DataAccess.Sql.Deduplication)),
                        index => AssertCreateSchema(modelChanges, index, nameof(GenericEndpoint.EventSourcing)),
                        index => AssertCreateSchema(modelChanges, index, nameof(GenericHost) + nameof(Test)),
                        index => AssertCreateSchema(modelChanges, index, nameof(DataAccess.Orm.Sql.Host.Migrations)),
                        index => AssertCreateEnumType(modelChanges, index, nameof(GenericHost) + nameof(Test), nameof(EnEnum), nameof(EnEnum.One), nameof(EnEnum.Two), nameof(EnEnum.Three)),
                        index => AssertCreateEnumType(modelChanges, index, nameof(GenericHost) + nameof(Test), nameof(EnEnumFlags), nameof(EnEnumFlags.A), nameof(EnEnumFlags.B), nameof(EnEnumFlags.C)),
                        index => AssertCreateEnumType(modelChanges, index, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(EnColumnConstraintType), nameof(EnColumnConstraintType.PrimaryKey), nameof(EnColumnConstraintType.ForeignKey)),
                        index => AssertCreateEnumType(modelChanges, index, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(EnTriggerEvent), nameof(EnTriggerEvent.Insert), nameof(EnTriggerEvent.Update), nameof(EnTriggerEvent.Delete)),
                        index => AssertCreateEnumType(modelChanges, index, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(EnTriggerType), nameof(EnTriggerType.Before), nameof(EnTriggerType.After)),
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericEndpoint.DataAccess.Sql.Deduplication),
                                typeof(GenericEndpoint.DataAccess.Sql.Deduplication.IntegrationMessage),
                                new[]
                                {
                                    (nameof(GenericEndpoint.DataAccess.Sql.Deduplication.IntegrationMessage.PrimaryKey), "not null primary key"),
                                    (nameof(GenericEndpoint.DataAccess.Sql.Deduplication.IntegrationMessage.Version), "not null"),
                                    (nameof(GenericEndpoint.DataAccess.Sql.Deduplication.IntegrationMessage.Payload), "not null"),
                                    (nameof(GenericEndpoint.DataAccess.Sql.Deduplication.IntegrationMessage.ReflectedType), "not null")
                                },
                                new[]
                                {
                                    nameof(GenericEndpoint.DataAccess.Sql.Deduplication.IntegrationMessage.Headers)
                                });
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericEndpoint.DataAccess.Sql.Deduplication),
                                typeof(IntegrationMessageHeader),
                                new[]
                                {
                                    (nameof(IntegrationMessageHeader.PrimaryKey), "not null primary key"),
                                    (nameof(IntegrationMessageHeader.Version), "not null"),
                                    (nameof(IntegrationMessageHeader.Message), $@"not null references ""{nameof(GenericEndpoint.DataAccess.Sql.Deduplication)}"".""{nameof(GenericEndpoint.DataAccess.Sql.Deduplication.IntegrationMessage)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade"),
                                    (nameof(IntegrationMessageHeader.Payload), "not null")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericDomain.EventSourcing),
                                typeof(DatabaseDomainEvent),
                                new[]
                                {
                                    (nameof(DatabaseDomainEvent.PrimaryKey), "not null primary key"),
                                    (nameof(DatabaseDomainEvent.Version), "not null"),
                                    (nameof(DatabaseDomainEvent.AggregateId), "not null"),
                                    (nameof(DatabaseDomainEvent.Index), "not null"),
                                    (nameof(DatabaseDomainEvent.Timestamp), "not null"),
                                    (nameof(DatabaseDomainEvent.DomainEvent), "not null")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericHost) + nameof(Test),
                                typeof(Blog),
                                new[]
                                {
                                    (nameof(Blog.PrimaryKey), "not null primary key"),
                                    (nameof(Blog.Version), "not null"),
                                    (nameof(Blog.Theme), "not null")
                                },
                                new[]
                                {
                                    nameof(Blog.Posts)
                                });
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericHost) + nameof(Test),
                                typeof(Community),
                                new[]
                                {
                                    (nameof(Community.PrimaryKey), "not null primary key"),
                                    (nameof(Community.Version), "not null"),
                                    (nameof(Community.Name), "not null")
                                },
                                new[]
                                {
                                    nameof(Community.Participants)
                                });
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericHost) + nameof(Test),
                                typeof(DatabaseEntity),
                                new[]
                                {
                                    (nameof(DatabaseEntity.PrimaryKey), "not null primary key"),
                                    (nameof(DatabaseEntity.Version), "not null"),
                                    (nameof(DatabaseEntity.BooleanField), "not null"),
                                    (nameof(DatabaseEntity.StringField), "not null"),
                                    (nameof(DatabaseEntity.NullableStringField), string.Empty),
                                    (nameof(DatabaseEntity.IntField), "not null"),
                                    (nameof(DatabaseEntity.Enum), "not null")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericHost) + nameof(Test),
                                typeof(Participant),
                                new[]
                                {
                                    (nameof(Participant.PrimaryKey), "not null primary key"),
                                    (nameof(Participant.Version), "not null"),
                                    (nameof(Participant.Name), "not null")
                                },
                                new[]
                                {
                                    nameof(Participant.Communities)
                                });
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericHost) + nameof(Test),
                                typeof(User),
                                new[]
                                {
                                    (nameof(User.PrimaryKey), "not null primary key"),
                                    (nameof(User.Version), "not null"),
                                    (nameof(User.Nickname), "not null")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(DataAccess.Orm.Sql.Host.Migrations),
                                typeof(AppliedMigration),
                                new[]
                                {
                                    (nameof(AppliedMigration.PrimaryKey), "not null primary key"),
                                    (nameof(AppliedMigration.Version), "not null"),
                                    (nameof(AppliedMigration.DateTime), "not null"),
                                    (nameof(AppliedMigration.CommandText), "not null"),
                                    (nameof(AppliedMigration.Name), "not null")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(DataAccess.Orm.Sql.Host.Migrations),
                                typeof(FunctionView),
                                new[]
                                {
                                    (nameof(FunctionView.PrimaryKey), "not null primary key"),
                                    (nameof(FunctionView.Version), "not null"),
                                    (nameof(FunctionView.Schema), "not null"),
                                    (nameof(FunctionView.Function), "not null"),
                                    (nameof(FunctionView.Definition), "not null")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(DataAccess.Orm.Sql.Host.Migrations),
                                typeof(SqlView),
                                new[]
                                {
                                    (nameof(SqlView.PrimaryKey), "not null primary key"),
                                    (nameof(SqlView.Version), "not null"),
                                    (nameof(SqlView.Schema), "not null"),
                                    (nameof(SqlView.View), "not null"),
                                    (nameof(SqlView.Query), "not null")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericEndpoint.DataAccess.Sql.Deduplication),
                                typeof(InboxMessage),
                                new[]
                                {
                                    (nameof(InboxMessage.PrimaryKey), "not null primary key"),
                                    (nameof(InboxMessage.Version), "not null"),
                                    (nameof(InboxMessage.Message), $@"not null references ""{nameof(GenericEndpoint.DataAccess.Sql.Deduplication)}"".""{nameof(IntegrationMessage)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade"),
                                    (nameof(InboxMessage.EndpointLogicalName), "not null"),
                                    (nameof(InboxMessage.EndpointInstanceName), "not null"),
                                    (nameof(InboxMessage.IsError), "not null"),
                                    (nameof(InboxMessage.Handled), "not null")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateMtmTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericEndpoint.DataAccess.Sql.Deduplication),
                                $"{nameof(IntegrationMessage)}_{nameof(IntegrationMessageHeader)}",
                                new[]
                                {
                                    (nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), $@"not null references ""{nameof(GenericEndpoint.DataAccess.Sql.Deduplication)}"".""{nameof(IntegrationMessage)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade"),
                                    (nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), $@"not null references ""{nameof(GenericEndpoint.DataAccess.Sql.Deduplication)}"".""{nameof(IntegrationMessageHeader)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade")
                                });
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericEndpoint.DataAccess.Sql.Deduplication),
                                typeof(OutboxMessage),
                                new[]
                                {
                                    (nameof(OutboxMessage.PrimaryKey), "not null primary key"),
                                    (nameof(OutboxMessage.Version), "not null"),
                                    (nameof(OutboxMessage.OutboxId), "not null"),
                                    (nameof(OutboxMessage.Timestamp), "not null"),
                                    (nameof(OutboxMessage.EndpointLogicalName), "not null"),
                                    (nameof(OutboxMessage.EndpointInstanceName), "not null"),
                                    (nameof(OutboxMessage.Message), $@"not null references ""{nameof(GenericEndpoint.DataAccess.Sql.Deduplication)}"".""{nameof(IntegrationMessage)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade"),
                                    (nameof(OutboxMessage.Sent), "not null")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateMtmTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericHost) + nameof(Test),
                                $"{nameof(Community)}_{nameof(Participant)}",
                                new[]
                                {
                                    (nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Community)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade"),
                                    (nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Participant)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade")
                                });
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericHost) + nameof(Test),
                                typeof(ComplexDatabaseEntity),
                                new[]
                                {
                                    (nameof(ComplexDatabaseEntity.PrimaryKey), "not null primary key"),
                                    (nameof(ComplexDatabaseEntity.Version), "not null"),
                                    (nameof(ComplexDatabaseEntity.Number), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableNumber), string.Empty),
                                    (nameof(ComplexDatabaseEntity.Identifier), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableIdentifier), string.Empty),
                                    (nameof(ComplexDatabaseEntity.Boolean), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableBoolean), string.Empty),
                                    (nameof(ComplexDatabaseEntity.DateTime), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableDateTime), string.Empty),
                                    (nameof(ComplexDatabaseEntity.TimeSpan), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableTimeSpan), string.Empty),
                                    (nameof(ComplexDatabaseEntity.DateOnly), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableDateOnly), string.Empty),
                                    (nameof(ComplexDatabaseEntity.TimeOnly), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableTimeOnly), string.Empty),
                                    (nameof(ComplexDatabaseEntity.ByteArray), "not null"),
                                    (nameof(ComplexDatabaseEntity.String), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableString), string.Empty),
                                    (nameof(ComplexDatabaseEntity.Enum), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableEnum), string.Empty),
                                    (nameof(ComplexDatabaseEntity.EnumFlags), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableEnumFlags), string.Empty),
                                    (nameof(ComplexDatabaseEntity.EnumArray), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableEnumArray), "not null"),
                                    (nameof(ComplexDatabaseEntity.StringArray), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableStringArray), "not null"),
                                    (nameof(ComplexDatabaseEntity.DateTimeArray), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableDateTimeArray), "not null"),
                                    (nameof(ComplexDatabaseEntity.Json), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableJson), string.Empty),
                                    (nameof(ComplexDatabaseEntity.Relation), $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Blog)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete no action"),
                                    (nameof(ComplexDatabaseEntity.NullableRelation), $@"references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Blog)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete no action")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericHost) + nameof(Test),
                                typeof(Post),
                                new[]
                                {
                                    (nameof(Post.PrimaryKey), "not null primary key"),
                                    (nameof(Post.Version), "not null"),
                                    (nameof(Post.DateTime), "not null"),
                                    (nameof(Post.Text), "not null"),
                                    (nameof(Post.Blog), $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Blog)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade"),
                                    (nameof(Post.User), $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(User)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete restrict")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateMtmTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericHost) + nameof(Test),
                                $"{nameof(Blog)}_{nameof(Post)}",
                                new[]
                                {
                                    (nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Blog)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade"),
                                    (nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Post)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade")
                                });
                        },
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseColumn)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseColumnConstraint)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseEnumType)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseFunction)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseIndexColumn)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseSchema)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseTrigger)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseView)),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(GenericEndpoint.DataAccess.Sql.Deduplication), $"{nameof(IntegrationMessage)}_{nameof(IntegrationMessageHeader)}", new[] { nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(GenericEndpoint.EventSourcing), nameof(DatabaseDomainEvent), new[] { nameof(DatabaseDomainEvent.AggregateId), nameof(DatabaseDomainEvent.Index) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, false, null, nameof(GenericEndpoint.EventSourcing), nameof(DatabaseDomainEvent), new[] { nameof(DatabaseDomainEvent.DomainEvent) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(GenericHost) + nameof(Test), $"{nameof(Blog)}_{nameof(Post)}", new[] { nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(GenericHost) + nameof(Test), $"{nameof(Community)}_{nameof(Participant)}", new[] { nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, false, $@"""{nameof(DatabaseEntity.BooleanField)}""",  nameof(GenericHost) + nameof(Test), nameof(DatabaseEntity), new[] { nameof(DatabaseEntity.StringField) }, new[] { nameof(DatabaseEntity.IntField) }),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(AppliedMigration), new[] { nameof(AppliedMigration.Name) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(DatabaseColumn), new[] { nameof(DatabaseColumn.Column), nameof(DatabaseColumn.Schema), nameof(DatabaseColumn.Table) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(DatabaseEnumType), new[] { nameof(DatabaseView.Schema), nameof(DatabaseEnumType.Type), nameof(DatabaseEnumType.Value) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(DatabaseFunction), new[] { nameof(DatabaseFunction.Function), nameof(DatabaseFunction.Schema) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(DatabaseIndexColumn), new[] { nameof(DatabaseIndexColumn.Column), nameof(DatabaseIndexColumn.Index), nameof(DatabaseIndexColumn.Schema), nameof(DatabaseIndexColumn.Table) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(DatabaseSchema), new[] { nameof(DatabaseSchema.Name) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(DatabaseTrigger), new[] { nameof(DatabaseTrigger.Schema), nameof(DatabaseTrigger.Trigger) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(DatabaseView), new[] { nameof(DatabaseView.Schema), nameof(DatabaseView.View) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(FunctionView), new[] { nameof(FunctionView.Function), nameof(FunctionView.Schema) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(SqlView), new[] { nameof(SqlView.Schema), nameof(SqlView.View) }, Array.Empty<string>()),
                        index => AssertCreateFunction(modelProvider, modelChanges, index, nameof(GenericEndpoint.EventSourcing), nameof(AppendOnlyAttribute)),
                        index => AssertCreateTrigger(modelProvider, modelChanges, index, nameof(GenericEndpoint.EventSourcing), $"{nameof(DatabaseDomainEvent)}_aotrg", nameof(AppendOnlyAttribute))
                    };

                    Assert.Equal(assertions.Length, modelChanges.Length);

                    for (var i = 0; i < assertions.Length; i++)
                    {
                        assertions[i](i);
                    }

                    static void AssertCreateTable(
                        IModelProvider modelProvider,
                        IModelChange[] modelChanges,
                        int index,
                        string schema,
                        Type table,
                        (string column, string constraints)[] columnsAssertions,
                        string[] mtmColumnsAssertions)
                    {
                        Assert.True(modelChanges[index] is CreateTable);
                        var createTable = (CreateTable)modelChanges[index];
                        Assert.Equal($"{schema}.{table.Name}", $"{createTable.Schema}.{createTable.Table}");

                        AssertColumns(modelProvider, modelChanges, index, columnsAssertions);
                        AssertMtmColumns(modelProvider, modelChanges, index, mtmColumnsAssertions);
                    }

                    static void AssertCreateMtmTable(
                        IModelProvider modelProvider,
                        IModelChange[] modelChanges,
                        int index,
                        string schema,
                        string table,
                        (string column, string constraints)[] columnsAssertions)
                    {
                        Assert.True(modelChanges[index] is CreateTable);
                        var createTable = (CreateTable)modelChanges[index];
                        Assert.Equal($"{schema}.{table}", $"{createTable.Schema}.{createTable.Table}");

                        AssertColumns(modelProvider, modelChanges, index, columnsAssertions);
                        AssertMtmColumns(modelProvider, modelChanges, index, Array.Empty<string>());
                    }

                    static void AssertColumns(
                        IModelProvider modelProvider,
                        IModelChange[] modelChanges,
                        int index,
                        (string column, string constraints)[] assertions)
                    {
                        Assert.True(modelChanges[index] is CreateTable);
                        var createTable = (CreateTable)modelChanges[index];
                        Assert.True(modelProvider.TablesMap.ContainsKey(createTable.Schema));
                        Assert.True(modelProvider.TablesMap[createTable.Schema].ContainsKey(createTable.Table));
                        Assert.True(modelProvider.TablesMap[createTable.Schema][createTable.Table] is TableInfo);
                        var tableInfo = (TableInfo)modelProvider.TablesMap[createTable.Schema][createTable.Table];
                        Assert.Equal(tableInfo.Columns.Values.Count(column => !column.IsMultipleRelation), assertions.Length);

                        foreach (var (column, constraints) in assertions)
                        {
                            Assert.True(tableInfo.Columns.ContainsKey(column));
                            var columnInfo = tableInfo.Columns[column];
                            var actualConstraints = columnInfo.Constraints.ToString(" ");
                            Assert.Equal(actualConstraints, constraints, ignoreCase: true);
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
                    }

                    static void AssertMtmColumns(
                        IModelProvider modelProvider,
                        IModelChange[] modelChanges,
                        int index,
                        string[] columns)
                    {
                        Assert.True(modelChanges[index] is CreateTable);
                        var createTable = (CreateTable)modelChanges[index];
                        Assert.True(modelProvider.TablesMap.ContainsKey(createTable.Schema));
                        Assert.True(modelProvider.TablesMap[createTable.Schema].ContainsKey(createTable.Table));
                        Assert.True(modelProvider.TablesMap[createTable.Schema][createTable.Table] is TableInfo);
                        var tableInfo = (TableInfo)modelProvider.TablesMap[createTable.Schema][createTable.Table];
                        Assert.Equal(tableInfo.Columns.Values.Count(column => column.IsMultipleRelation), columns.Length);

                        foreach (var column in columns)
                        {
                            Assert.True(tableInfo.Columns.ContainsKey(column));
                            var columnInfo = tableInfo.Columns[column];
                            Assert.True(columnInfo.IsMultipleRelation);
                            Assert.NotNull(columnInfo.Relation);
                        }
                    }
                }
                else
                {
                    throw new NotSupportedException(databaseConnectionProvider.GetType().FullName);
                }

                static void AssertCreateSchema(
                    IModelChange[] modelChanges,
                    int index,
                    string schema)
                {
                    Assert.True(modelChanges[index] is CreateSchema);
                    var createSchema = (CreateSchema)modelChanges[index];
                    Assert.Equal(createSchema.Schema, schema, ignoreCase: true);
                }

                static void AssertCreateEnumType(
                    IModelChange[] modelChanges,
                    int index,
                    string schema,
                    string type,
                    params string[] values)
                {
                    Assert.True(modelChanges[index] is CreateEnumType);
                    var createEnumType = (CreateEnumType)modelChanges[index];
                    Assert.Equal(createEnumType.Schema, schema, ignoreCase: true);
                    Assert.Equal(createEnumType.Type, type, ignoreCase: true);
                    Assert.True(createEnumType.Values.SequenceEqual(values, StringComparer.Ordinal));
                }

                static void AssertCreateView(
                    IModelChange[] modelChanges,
                    int index,
                    string view)
                {
                    Assert.True(modelChanges[index] is CreateView);
                    var createView = (CreateView)modelChanges[index];
                    Assert.Equal(createView.View, view, ignoreCase: true);
                }

                static void AssertCreateIndex(
                    IModelProvider modelProvider,
                    IModelChange[] modelChanges,
                    int index,
                    bool unique,
                    string? predicate,
                    string schema,
                    string table,
                    string[] columns,
                    string[] includedColumns)
                {
                    Assert.True(modelChanges[index] is CreateIndex);
                    var createIndex = (CreateIndex)modelChanges[index];
                    Assert.Equal(createIndex.Schema, schema, ignoreCase: true);
                    Assert.Equal(createIndex.Table, table, ignoreCase: true);
                    var indexName = string.Join("__", table, string.Join("_", columns));
                    Assert.Equal(createIndex.Index, indexName, ignoreCase: true);
                    var indexInfo = modelProvider.TablesMap[schema][table].Indexes[createIndex.Index];
                    Assert.True(includedColumns.OrderBy(column => column).SequenceEqual(indexInfo.IncludedColumns.Select(column => column.Name).OrderBy(column => column), StringComparer.OrdinalIgnoreCase));
                    Assert.Equal(unique, indexInfo.Unique);
                    Assert.Equal(predicate, indexInfo.Predicate);
                }

                static void AssertCreateFunction(
                    IModelProvider modelProvider,
                    IModelChange[] modelChanges,
                    int index,
                    string schema,
                    string function)
                {
                    Assert.True(modelChanges[index] is CreateFunction);
                    var createFunction = (CreateFunction)modelChanges[index];
                    Assert.Equal(createFunction.Schema, schema, ignoreCase: true);
                    Assert.Equal(createFunction.Function, function, ignoreCase: true);
                }

                static void AssertCreateTrigger(
                    IModelProvider modelProvider,
                    IModelChange[] modelChanges,
                    int index,
                    string schema,
                    string trigger,
                    string function)
                {
                    Assert.True(modelChanges[index] is CreateTrigger);
                    var createTrigger = (CreateTrigger)modelChanges[index];
                    Assert.Equal(createTrigger.Schema, schema, ignoreCase: true);
                    Assert.Equal(createTrigger.Trigger, trigger, ignoreCase: true);
                    Assert.Equal(createTrigger.Function, function, ignoreCase: true);
                }
            }
        }
    }
}