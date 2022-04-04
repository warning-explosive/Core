namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using CompositionRoot.Api.Exceptions;
    using CompositionRoot.Api.Extensions;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using DataAccess.Orm.PostgreSql.Host;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.DataAccess.BackgroundWorkers;
    using GenericEndpoint.DataAccess.StartupActions;
    using GenericEndpoint.Endpoint;
    using GenericEndpoint.Host;
    using GenericEndpoint.Host.StartupActions;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Pipeline;
    using GenericEndpoint.Tracing.Pipeline;
    using GenericHost;
    using GenericHost.Api.Abstractions;
    using IntegrationTransport.Api.Abstractions;
    using IntegrationTransport.Host;
    using IntegrationTransport.Host.BackgroundWorkers;
    using IntegrationTransport.RpcRequest;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Overrides;
    using TracingEndpoint.Contract.Messages;
    using Xunit;
    using Xunit.Abstractions;
    using IIntegrationContext = IntegrationTransport.Api.Abstractions.IIntegrationContext;

    /// <summary>
    /// GenericHost assembly tests
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "reviewed")]
    public class GenericHostBuildHostTest : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public GenericHostBuildHostTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// useContainer; useTransport;
        /// </summary>
        /// <returns>BuildHostTestData</returns>
        public static IEnumerable<object[]> BuildHostTestData()
        {
            var useInMemoryIntegrationTransport = new Func<ITestOutputHelper, IHostBuilder, IHostBuilder>(
                (output, hostBuilder) => hostBuilder
                   .UseIntegrationTransport(builder => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .WithTracing()
                       .ModifyContainerOptions(options => options.WithOverrides(new TestLoggerOverride(output)))
                       .BuildOptions()));

            return new[]
            {
                new object[]
                {
                    useInMemoryIntegrationTransport
                }
            };
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostTestData))]
        internal void SameTransportTest(Func<ITestOutputHelper, IHostBuilder, IHostBuilder> useTransport)
        {
            var host = useTransport(Output, Host.CreateDefaultBuilder())
                .UseEndpoint(
                    TestIdentity.Endpoint10,
                    (_, builder) => builder
                        .WithTracing()
                        .ModifyContainerOptions(options => options.WithOverrides(new TestLoggerOverride(Output)))
                        .BuildOptions())
                .UseEndpoint(
                    TestIdentity.Endpoint20,
                    (_, builder) => builder
                        .WithTracing()
                        .ModifyContainerOptions(options => options.WithOverrides(new TestLoggerOverride(Output)))
                        .BuildOptions())
                .BuildHost();

            var transport = host.GetTransportDependencyContainer().Resolve<IIntegrationTransport>();

            var transportIsSame = new[]
                {
                    host.GetEndpointDependencyContainer(TestIdentity.Endpoint10),
                    host.GetEndpointDependencyContainer(TestIdentity.Endpoint20)
                }
                .Select(container => container.Resolve<IIntegrationTransport>())
                .All(endpointTransport => ReferenceEquals(transport, endpointTransport));

            Assert.True(transportIsSame);
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostTestData))]
        internal void BuildHostTest(Func<ITestOutputHelper, IHostBuilder, IHostBuilder> useTransport)
        {
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
                typeof(IntegrationEventEmptyMessageHandler),
                typeof(BaseEventEmptyMessageHandler),
                typeof(InheritedEventEmptyMessageHandler),
                typeof(CommandEmptyMessageHandler),
                typeof(OpenGenericCommandEmptyMessageHandler<>),
                typeof(QueryAlwaysReplyMessageHandler),
                typeof(ReplyEmptyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = useTransport(Output, Host.CreateDefaultBuilder())
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .WithDataAccess(new PostgreSqlDatabaseProvider())
                       .WithTracing()
                       .ModifyContainerOptions(options => options
                           .WithOverrides(new TestLoggerOverride(Output))
                           .WithAdditionalOurTypes(additionalOurTypes))
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
                    typeof(GenericEndpointOutboxHostStartupAction),
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
                    typeof(GenericEndpointOutboxHostBackgroundWorker),
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
                var endpointDependencyContainer = host.GetEndpointDependencyContainer(endpointIdentity);
                var integrationMessage = new IntegrationMessage(new Command(0), typeof(Command));

                Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage));
                Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<Core.GenericEndpoint.Api.Abstractions.IIntegrationContext>());

                using (endpointDependencyContainer.OpenScope())
                {
                    Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                    var advancedIntegrationContext = endpointDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage);

                    var expectedContexts = new[]
                    {
                        typeof(AdvancedIntegrationContext)
                    };

                    var actualContexts = advancedIntegrationContext
                        .FlattenDecoratedType()
                        .ShowTypes("extended integration context", log)
                        .ToList();

                    Assert.Equal(expectedContexts, actualContexts);

                    var expectedPipeline = new[]
                    {
                        typeof(ErrorHandlingPipeline),
                        typeof(UnitOfWorkPipeline),
                        typeof(TracingPipeline),
                        typeof(QueryReplyValidationPipeline),
                        typeof(MessageHandlerPipeline),
                    };

                    var actualPipeline = endpointDependencyContainer
                        .Resolve<IMessagePipeline>()
                        .FlattenDecoratedType()
                        .ShowTypes("message pipeline", log)
                        .ToList();

                    Assert.Equal(expectedPipeline, actualPipeline);

                    var integrationTypeProvider = endpointDependencyContainer.Resolve<IIntegrationTypeProvider>();

                    var expectedIntegrationMessageTypes = new[]
                    {
                        typeof(IIntegrationMessage),
                        typeof(IIntegrationCommand),
                        typeof(IIntegrationEvent),
                        typeof(IIntegrationReply),
                        typeof(IIntegrationQuery<>),
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
                        typeof(IIntegrationEvent),
                        typeof(BaseEvent),
                        typeof(InheritedEvent)
                    };

                    var actualEvents = integrationTypeProvider
                        .EventsSubscriptions()
                        .ShowTypes(nameof(IIntegrationTypeProvider.EventsSubscriptions), log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedEvents.OrderBy(type => type.FullName).ToList(), actualEvents);

                    var expectedCommandHandlers = new[]
                    {
                        typeof(CommandEmptyMessageHandler)
                    };

                    var actualCommandHandlers = endpointDependencyContainer
                        .ResolveCollection<IMessageHandler<Command>>()
                        .Select(handler => handler.GetType())
                        .ShowTypes("actualCommandHandlers", log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedCommandHandlers.OrderBy(type => type.FullName).ToList(), actualCommandHandlers);

                    var expectedOpenGenericHandlerCommandHandlers = new[]
                    {
                        typeof(OpenGenericCommandEmptyMessageHandler<OpenGenericHandlerCommand>)
                    };

                    var actualOpenGenericHandlerCommandHandlers = endpointDependencyContainer
                        .ResolveCollection<IMessageHandler<OpenGenericHandlerCommand>>()
                        .Select(handler => handler.GetType())
                        .ShowTypes("actualOpenGenericHandlerCommandHandlers", log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(actualOpenGenericHandlerCommandHandlers.OrderBy(type => type.FullName).ToList(), expectedOpenGenericHandlerCommandHandlers);

                    var expectedBaseEventHandlers = new[]
                    {
                        typeof(IntegrationEventEmptyMessageHandler),
                        typeof(BaseEventEmptyMessageHandler)
                    };

                    var actualBaseEventHandlers = endpointDependencyContainer
                        .ResolveCollection<IMessageHandler<BaseEvent>>()
                        .Select(handler => handler.GetType())
                        .ShowTypes("actualBaseEventHandlers", log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedBaseEventHandlers.OrderBy(type => type.FullName).ToList(), actualBaseEventHandlers);

                    var expectedFirstInheritedEventHandlers = new[]
                    {
                        typeof(IntegrationEventEmptyMessageHandler),
                        typeof(BaseEventEmptyMessageHandler),
                        typeof(InheritedEventEmptyMessageHandler)
                    };

                    var actualFirstInheritedEventHandlers = endpointDependencyContainer
                        .ResolveCollection<IMessageHandler<InheritedEvent>>()
                        .Select(handler => handler.GetType())
                        .ShowTypes("actualInheritedEventHandlers", log)
                        .OrderBy(type => type.FullName)
                        .ToList();

                    Assert.Equal(expectedFirstInheritedEventHandlers.OrderBy(type => type.FullName).ToList(), actualFirstInheritedEventHandlers);
                }
            }

            static void CheckTransport(IHost host, Action<string> log)
            {
                var transportDependencyContainer = host.GetTransportDependencyContainer();

                _ = transportDependencyContainer.Resolve<IIntegrationTransport>();

                var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                var expectedContexts = new[]
                {
                    typeof(SpaceEngineers.Core.IntegrationTransport.Integration.IntegrationContext)
                };

                var actualContexts = integrationContext
                    .FlattenDecoratedType()
                    .ShowTypes("transport integration context", log)
                    .ToList();

                Assert.Equal(expectedContexts, actualContexts);

                _ = transportDependencyContainer.Resolve<IRpcRequestRegistry>();

                var expectedRpcReplyHandlers = new[]
                {
                    typeof(SpaceEngineers.Core.IntegrationTransport.RpcRequest.ErrorHandlingRpcReplyMessageHandler<IIntegrationReply>),
                    typeof(SpaceEngineers.Core.IntegrationTransport.Tracing.RpcRequest.TracingRpcReplyMessageHandler<IIntegrationReply>),
                    typeof(SpaceEngineers.Core.IntegrationTransport.RpcRequest.RpcReplyMessageHandler<IIntegrationReply>)
                };

                var actualRpcReplyHandlers = transportDependencyContainer
                    .Resolve<IRpcReplyMessageHandler<IIntegrationReply>>()
                    .FlattenDecoratedType()
                    .ShowTypes("rpc reply handlers", log)
                    .ToList();

                Assert.Equal(expectedRpcReplyHandlers, actualRpcReplyHandlers);
            }
        }
    }
}