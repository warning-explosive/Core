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
    using DataAccess.Api.Model;
    using DataAccess.Orm.Connection;
    using DataAccess.Orm.PostgreSql.Host.StartupActions;
    using DataAccess.Orm.Sql.Host.Model;
    using DataAccess.Orm.Sql.Model;
    using DataAccess.Orm.Transaction;
    using DatabaseEntities.Relations;
    using GenericDomain.EventSourcing.Sql;
    using GenericEndpoint.Authorization;
    using GenericEndpoint.Authorization.Host;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.DataAccess.Deduplication;
    using GenericEndpoint.DataAccess.Host;
    using GenericEndpoint.DataAccess.Host.BackgroundWorkers;
    using GenericEndpoint.DataAccess.Host.StartupActions;
    using GenericEndpoint.EventSourcing;
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
            var settingsDirectory = SolutionExtensions
               .ProjectFile()
               .Directory
               .EnsureNotNull("Project directory wasn't found")
               .StepInto("Settings")
               .StepInto(nameof(BuildHostTest));

            var useInMemoryIntegrationTransport = new Func<IHostBuilder, IHostBuilder>(
                hostBuilder => hostBuilder
                   .UseIntegrationTransport(builder => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .BuildOptions()));

            var useRabbitMqIntegrationTransport = new Func<IHostBuilder, IHostBuilder>(
                hostBuilder => hostBuilder
                   .UseIntegrationTransport(builder => builder
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

            var settingsDirectory = SolutionExtensions
               .ProjectFile()
               .Directory
               .EnsureNotNull("Project directory wasn't found")
               .StepInto("Settings")
               .StepInto(nameof(BuildHostTest));

            var useInMemoryIntegrationTransport = new Func<IHostBuilder, IHostBuilder>(
                static hostBuilder => hostBuilder
                   .UseIntegrationTransport(builder => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .BuildOptions()));

            var useRabbitMqIntegrationTransport = new Func<IHostBuilder, IHostBuilder>(
                static hostBuilder => hostBuilder
                   .UseIntegrationTransport(builder => builder
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
                    (_, builder) => builder.BuildOptions())
                .UseEndpoint(
                    TestIdentity.Endpoint20,
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
                    (_, builder) => builder
                       .WithPostgreSqlDataAccess(options => options
                           .ExecuteMigrations())
                       .WithSqlEventSourcing()
                       .WithAuthorization()
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
                            typeof(CaptureDomainEvent<,>),
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
                            typeof(CaptureDomainEvent<,>),
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
                            typeof(CaptureDomainEvent<,>),
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

                var expectedUserScopeProviders = new[]
                {
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
                        (transaction, token) => endpointContainer.Resolve<IDatabaseModelBuilder>().BuildModel(transaction, token),
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

                var actualModel = await endpointContainer.InvokeWithinTransaction(
                        false,
                        (transaction, token) => endpointContainer.Resolve<IDatabaseModelBuilder>().BuildModel(transaction, token),
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
                        index => AssertCreateSchema(modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication)),
                        index => AssertCreateSchema(modelChanges, index, nameof(GenericEndpoint.EventSourcing)),
                        index => AssertCreateSchema(modelChanges, index, nameof(GenericHost) + nameof(Test)),
                        index => AssertCreateSchema(modelChanges, index, nameof(DataAccess.Orm.Sql.Host.Migrations)),
                        index => AssertCreateEnumType(modelChanges, index, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(EnColumnConstraintType), nameof(EnColumnConstraintType.PrimaryKey), nameof(EnColumnConstraintType.ForeignKey)),
                        index =>
                        {
                            AssertCreateTable(modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication), typeof(IntegrationMessageHeader));
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(IntegrationMessageHeader.PrimaryKey), "not null primary key");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(IntegrationMessageHeader.Version), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(IntegrationMessageHeader.Payload), "not null");
                        },
                        index =>
                        {
                            AssertCreateTable(modelChanges, index, nameof(GenericDomain.EventSourcing), typeof(DatabaseDomainEvent));
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(DatabaseDomainEvent.PrimaryKey), "not null primary key");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(DatabaseDomainEvent.Version), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(DatabaseDomainEvent.AggregateId), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(DatabaseDomainEvent.Index), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(DatabaseDomainEvent.Timestamp), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(DatabaseDomainEvent.DomainEvent), "not null");
                        },
                        index =>
                        {
                            AssertCreateTable(modelChanges, index, nameof(GenericHost) + nameof(Test), typeof(Blog));
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(Blog.PrimaryKey), "not null primary key");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(Blog.Version), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(Blog.Theme), "not null");
                            AssertMtmColumn(endpointContainer, modelChanges, index, $"{nameof(Blog.Posts)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}");
                        },
                        index =>
                        {
                            AssertCreateTable(modelChanges, index, nameof(GenericHost) + nameof(Test), typeof(Community));
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(Community.PrimaryKey), "not null primary key");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(Community.Version), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(Community.Name), "not null");
                            AssertMtmColumn(endpointContainer, modelChanges, index, $"{nameof(Community.Participants)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}");
                        },
                        index =>
                        {
                            AssertCreateTable(modelChanges, index, nameof(GenericHost) + nameof(Test), typeof(Participant));
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(Participant.PrimaryKey), "not null primary key");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(Participant.Version), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(Participant.Name), "not null");
                            AssertMtmColumn(endpointContainer, modelChanges, index, $"{nameof(Participant.Communities)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right)}");
                        },
                        index =>
                        {
                            AssertCreateTable(modelChanges, index, nameof(GenericHost) + nameof(Test), typeof(User));
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(User.PrimaryKey), "not null primary key");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(User.Version), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(User.Nickname), "not null");
                        },
                        index =>
                        {
                            AssertCreateTable(modelChanges, index, nameof(DataAccess.Orm.Sql.Host.Migrations), typeof(AppliedMigration));
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(AppliedMigration.PrimaryKey), "not null primary key");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(AppliedMigration.Version), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(AppliedMigration.DateTime), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(AppliedMigration.CommandText), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(AppliedMigration.Name), "not null");
                        },
                        index =>
                        {
                            AssertCreateTable(modelChanges, index, nameof(DataAccess.Orm.Sql.Host.Migrations), typeof(SqlView));
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(SqlView.PrimaryKey), "not null primary key");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(SqlView.Version), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(SqlView.Schema), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(SqlView.View), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(SqlView.Query), "not null");
                        },
                        index =>
                        {
                            AssertCreateTable(modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication), typeof(GenericEndpoint.DataAccess.Deduplication.IntegrationMessage));
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication.IntegrationMessage.PrimaryKey), "not null primary key");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication.IntegrationMessage.Version), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication.IntegrationMessage.Payload), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, $"{nameof(GenericEndpoint.DataAccess.Deduplication.IntegrationMessage.ReflectedType)}_{nameof(SystemType.Type)}", "not null");
                            AssertMtmColumn(endpointContainer, modelChanges, index, $"{nameof(GenericEndpoint.DataAccess.Deduplication.IntegrationMessage.Headers)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}");
                        },
                        index =>
                        {
                            AssertCreateMtmTable(modelChanges, index, nameof(GenericHost) + nameof(Test), $"{nameof(Community)}_{nameof(Participant)}");
                            AssertHasNoColumn(endpointContainer, modelChanges, index, nameof(IUniqueIdentified.PrimaryKey));
                            AssertHasNoColumn(endpointContainer, modelChanges, index, nameof(IDatabaseEntity.Version));
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Community)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Participant)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                        },
                        index =>
                        {
                            AssertCreateTable(modelChanges, index, nameof(GenericHost) + nameof(Test), typeof(Post));
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(Post.PrimaryKey), "not null primary key");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(Post.Version), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(Post.DateTime), "not null");
                            AssertHasNoColumn(endpointContainer, modelChanges, index, $"{nameof(Blog)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}");
                            AssertHasNoColumn(endpointContainer, modelChanges, index, $"{nameof(Blog)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right)}");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, $"{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}", $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Blog)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                        },
                        index =>
                        {
                            AssertCreateTable(modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication), typeof(InboxMessage));
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(InboxMessage.PrimaryKey), "not null primary key");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(InboxMessage.Version), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, $"{nameof(InboxMessage.Message)}_{nameof(InboxMessage.Message.PrimaryKey)}", $@"not null references ""{nameof(GenericEndpoint.DataAccess.Deduplication)}"".""{nameof(IntegrationMessage)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, $"{nameof(InboxMessage.EndpointIdentity)}_{nameof(GenericEndpoint.DataAccess.Deduplication.EndpointIdentity.LogicalName)}", "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, $"{nameof(InboxMessage.EndpointIdentity)}_{nameof(GenericEndpoint.DataAccess.Deduplication.EndpointIdentity.InstanceName)}", "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(InboxMessage.IsError), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(InboxMessage.Handled), "not null");
                        },
                        index =>
                        {
                            AssertCreateMtmTable(modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication), $"{nameof(IntegrationMessage)}_{nameof(IntegrationMessageHeader)}");
                            AssertHasNoColumn(endpointContainer, modelChanges, index, nameof(IUniqueIdentified.PrimaryKey));
                            AssertHasNoColumn(endpointContainer, modelChanges, index, nameof(IDatabaseEntity.Version));
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), $@"not null references ""{nameof(GenericEndpoint.DataAccess.Deduplication)}"".""{nameof(IntegrationMessage)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), $@"not null references ""{nameof(GenericEndpoint.DataAccess.Deduplication)}"".""{nameof(IntegrationMessageHeader)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                        },
                        index =>
                        {
                            AssertCreateTable(modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication), typeof(OutboxMessage));
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(OutboxMessage.PrimaryKey), "not null primary key");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(OutboxMessage.Version), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(OutboxMessage.OutboxId), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(OutboxMessage.Timestamp), "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, $"{nameof(OutboxMessage.EndpointIdentity)}_{nameof(GenericEndpoint.DataAccess.Deduplication.EndpointIdentity.LogicalName)}", "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, $"{nameof(OutboxMessage.EndpointIdentity)}_{nameof(GenericEndpoint.DataAccess.Deduplication.EndpointIdentity.InstanceName)}", "not null");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, $"{nameof(OutboxMessage.Message)}_{nameof(OutboxMessage.Message.PrimaryKey)}", $@"not null references ""{nameof(GenericEndpoint.DataAccess.Deduplication)}"".""{nameof(IntegrationMessage)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(OutboxMessage.Sent), "not null");
                        },
                        index =>
                        {
                            AssertCreateMtmTable(modelChanges, index, nameof(GenericHost) + nameof(Test), $"{nameof(Blog)}_{nameof(Post)}");
                            AssertHasNoColumn(endpointContainer, modelChanges, index, nameof(IUniqueIdentified.PrimaryKey));
                            AssertHasNoColumn(endpointContainer, modelChanges, index, nameof(IDatabaseEntity.Version));
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Blog)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                            AssertColumnConstraints(endpointContainer, modelChanges, index, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Post)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"")");
                        },
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseColumn)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseColumnConstraint)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseEnumType)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseIndex)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseSchema)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseView)),
                        index => AssertCreateIndex(modelChanges, index, nameof(GenericEndpoint.DataAccess.Deduplication), $"{nameof(IntegrationMessage)}_{nameof(IntegrationMessageHeader)}", $"{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right)}"),
                        index => AssertCreateIndex(modelChanges, index, nameof(GenericEndpoint.EventSourcing), nameof(DatabaseDomainEvent), $"{nameof(DatabaseDomainEvent.AggregateId)}_{nameof(DatabaseDomainEvent.Index)}"),
                        index => AssertCreateIndex(modelChanges, index, nameof(GenericHost) + nameof(Test), $"{nameof(Blog)}_{nameof(Post)}", $"{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right)}"),
                        index => AssertCreateIndex(modelChanges, index, nameof(GenericHost) + nameof(Test), $"{nameof(Community)}_{nameof(Participant)}", $"{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right)}"),
                        index => AssertCreateIndex(modelChanges, index, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(AppliedMigration), nameof(AppliedMigration.Name)),
                        index => AssertCreateIndex(modelChanges, index, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(DatabaseColumn), $"{nameof(DatabaseColumn.Column)}_{nameof(DatabaseColumn.Schema)}_{nameof(DatabaseColumn.Table)}"),
                        index => AssertCreateIndex(modelChanges, index, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(DatabaseEnumType), $"{nameof(DatabaseView.Schema)}_{nameof(DatabaseEnumType.Type)}_{nameof(DatabaseEnumType.Value)}"),
                        index => AssertCreateIndex(modelChanges, index, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(DatabaseIndex), $"{nameof(DatabaseIndex.Index)}_{nameof(DatabaseIndex.Schema)}_{nameof(DatabaseIndex.Table)}"),
                        index => AssertCreateIndex(modelChanges, index, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(DatabaseSchema), $"{nameof(DatabaseSchema.Name)}"),
                        index => AssertCreateIndex(modelChanges, index, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(DatabaseView), $"{nameof(DatabaseView.Schema)}_{nameof(DatabaseView.View)}"),
                        index => AssertCreateIndex(modelChanges, index, nameof(DataAccess.Orm.Sql.Host.Migrations), nameof(SqlView), $"{nameof(SqlView.Schema)}_{nameof(SqlView.View)}")
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
                    throw new NotSupportedException(databaseConnectionProvider.GetType().FullName);
                }

                static void AssertCreateSchema(IModelChange[] modelChanges, int index, string schema)
                {
                    Assert.True(modelChanges[index] is CreateSchema);
                    var createSchema = (CreateSchema)modelChanges[index];
                    Assert.True(createSchema.Schema.Equals(schema, StringComparison.OrdinalIgnoreCase));
                }

                static void AssertCreateEnumType(IModelChange[] modelChanges, int index, string schema, string type, params string[] values)
                {
                    Assert.True(modelChanges[index] is CreateEnumType);
                    var createEnumType = (CreateEnumType)modelChanges[index];
                    Assert.True(createEnumType.Schema.Equals(schema, StringComparison.OrdinalIgnoreCase));
                    Assert.True(createEnumType.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
                    Assert.True(createEnumType.Values.SequenceEqual(values, StringComparer.Ordinal));
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
}