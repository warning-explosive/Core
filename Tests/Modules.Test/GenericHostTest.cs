namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using CompositionRoot.Api.Exceptions;
    using CompositionRoot.Api.Extensions;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using DataAccess.Orm.Connection;
    using DataAccess.Orm.InMemoryDatabase;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Endpoint;
    using GenericEndpoint.Host;
    using GenericEndpoint.Host.StartupActions;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;
    using GenericEndpoint.Pipeline;
    using GenericEndpoint.Tracing.Pipeline;
    using GenericHost;
    using GenericHost.Api.Abstractions;
    using IntegrationTransport.Api.Abstractions;
    using IntegrationTransport.Host;
    using IntegrationTransport.Host.BackgroundWorkers;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Mocks;
    using Registrations;
    using TracingEndpoint.Contract;
    using TracingEndpoint.Contract.Messages;
    using TracingEndpoint.Host;
    using Xunit;
    using Xunit.Abstractions;
    using IIntegrationContext = IntegrationTransport.Api.Abstractions.IIntegrationContext;

    /// <summary>
    /// GenericHost assembly tests
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "For test reasons")]
    public class GenericHostTest : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public GenericHostTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// DependencyContainerTestData
        /// </summary>
        /// <returns>Dependency container producers</returns>
        public static IEnumerable<object[]> DependencyContainerTestData()
        {
            var dependencyContainerProducers = new object[]
            {
                new Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>>(options => options.UseGenericContainer())
            };

            return new[] { dependencyContainerProducers };
        }

        /// <summary>
        /// useContainer; useTransport;
        /// </summary>
        /// <returns>BuildHostTestData</returns>
        public static IEnumerable<object[]> BuildHostTestData()
        {
            var integrationTransportProviders = new object[]
            {
                new Func<IHostBuilder, IHostBuilder>(hostBuilder => hostBuilder
                    .UseIntegrationTransport(builder => builder
                        .WithInMemoryIntegrationTransport()
                        .WithDefaultCrossCuttingConcerns()
                        .BuildOptions())),
            };

            return DependencyContainerTestData()
                .SelectMany(useContainer => integrationTransportProviders
                    .Select(useTransport => useContainer.Concat(new[] { useTransport }).ToArray()));
        }

        /// <summary>
        /// useContainer; useTransport; timeout;
        /// </summary>
        /// <returns>RunHostTestData</returns>
        public static IEnumerable<object[]> RunHostTestData()
        {
            var timeout = TimeSpan.FromSeconds(60);

            var inMemoryIntegrationTransportCollector = new MessagesCollector();
            var useInMemoryIntegrationTransport = new Func<IHostBuilder, IHostBuilder>(hostBuilder => hostBuilder
                .UseIntegrationTransport(builder => builder
                    .WithInMemoryIntegrationTransport()
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithManualRegistrations(new MessagesCollectorInstanceManualRegistration(inMemoryIntegrationTransportCollector)))
                    .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                new object[] { useInMemoryIntegrationTransport, inMemoryIntegrationTransportCollector }
            };

            return DependencyContainerTestData()
                .SelectMany(useContainer => integrationTransportProviders
                    .Select(useTransport => useContainer.Concat(useTransport).Concat(new object[] { timeout }).ToArray()));
        }

        /// <summary>
        /// useContainer; useTransport; databaseProvider; timeout;
        /// </summary>
        /// <returns>RunHostWithDataAccessTestData</returns>
        public static IEnumerable<object[]> RunHostWithDataAccessTestData()
        {
            var timeout = TimeSpan.FromSeconds(300);

            var inMemoryIntegrationTransportCollector = new MessagesCollector();
            var useInMemoryIntegrationTransport = new Func<IHostBuilder, IHostBuilder>(hostBuilder => hostBuilder
                .UseIntegrationTransport(builder => builder
                    .WithInMemoryIntegrationTransport()
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithManualRegistrations(new MessagesCollectorInstanceManualRegistration(inMemoryIntegrationTransportCollector)))
                    .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                new object[] { useInMemoryIntegrationTransport, inMemoryIntegrationTransportCollector }
            };

            var databaseProviders = new object[]
            {
                // TODO: #135 - new PostgreSqlDatabaseProvider(),
                new InMemoryDatabaseProvider()
            };

            return DependencyContainerTestData()
                .SelectMany(useContainer => integrationTransportProviders
                    .SelectMany(useTransport => databaseProviders
                        .Select(databaseProvider => useContainer.Concat(useTransport).Concat(new[] { databaseProvider, timeout }).ToArray())));
        }

        [Fact(Timeout = 60_000)]
        /*[MemberData(nameof(RunHostWithDataAccessTestData))]*/
        internal void BuildDatabaseModelTest()
        {
            // TODO: #110
            /*var tracingEndpointIdentity = new EndpointIdentity(TracingEndpointIdentity.LogicalName, 0);

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseTracingEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithDataAccess(databaseProvider)
                    .BuildOptions(tracingEndpointIdentity))
                .BuildHost();

            var tracingEndpointContainer = host.GetEndpointDependencyContainer(tracingEndpointIdentity);

            var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var actualModel = await tracingEndpointContainer
                    .Resolve<IDatabaseModelBuilder>()
                    .BuildModel(cts.Token)
                    .ConfigureAwait(false);

                var expectedModel = await tracingEndpointContainer
                    .Resolve<ICodeModelBuilder>()
                    .BuildModel(cts.Token)
                    .ConfigureAwait(false);

                var modelChanges = tracingEndpointContainer
                    .Resolve<IDatabaseModelComparator>()
                    .ExtractDiff(actualModel, expectedModel)
                    .ToList();

                modelChanges.Each(change => Output.WriteLine(change.ToString()));
                Assert.NotEmpty(modelChanges);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }*/
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostWithDataAccessTestData))]
        internal async Task GetConversationTraceTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            MessagesCollector collector,
            IDatabaseProvider databaseProvider,
            TimeSpan timeout)
        {
            Output.WriteLine(databaseProvider.GetType().FullName);

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

            var tracingEndpointIdentity = new EndpointIdentity(TracingEndpointIdentity.LogicalName, 0);

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithTracing()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(additionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint10))
                .UseTracingEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithDataAccess(databaseProvider)
                    .BuildOptions(tracingEndpointIdentity))
                .BuildHost();

            var container = host.GetTransportDependencyContainer();

            var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var integrationContext = container.Resolve<IIntegrationContext>();

                var conversationId = Guid.NewGuid();

                var trace = await integrationContext
                    .RpcRequest<GetConversationTrace, ConversationTrace>(new GetConversationTrace(conversationId), cts.Token)
                    .ConfigureAwait(false);

                Assert.Empty(collector.ErrorMessages);
                Assert.Equal(2, collector.Messages.Count);
                var messages = collector.Messages.ToArray();
                Assert.Equal(typeof(GetConversationTrace), messages[0].Payload.GetType());
                Assert.Equal(typeof(ConversationTrace), messages[1].Payload.GetType());

                Assert.Equal(conversationId, trace.ConversationId);
                Assert.Null(trace.Message);
                Assert.Null(trace.RefuseReason);
                Assert.Null(trace.SubsequentTrace);

                var query = new Query(42);

                var awaiter = Task.WhenAll(collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.IntegrationMessage.ReflectedType == typeof(Query)));

                /*
                 * TODO: #157 - capture trace of RPC replies on the host side
                 * collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.IntegrationMessage.ReflectedType == typeof(Reply))
                 */

                var reply = await integrationContext
                    .RpcRequest<Query, Reply>(query, cts.Token)
                    .ConfigureAwait(false);

                await awaiter.ConfigureAwait(false);

                Assert.Empty(collector.ErrorMessages);
                Assert.Equal(5, collector.Messages.Count);
                messages = collector.Messages.ToArray();
                Assert.Equal(typeof(GetConversationTrace), messages[0].Payload.GetType());
                Assert.Equal(typeof(ConversationTrace), messages[1].Payload.GetType());
                Assert.Equal(typeof(Query), messages[2].Payload.GetType());
                Assert.Equal(typeof(Reply), messages[3].Payload.GetType());
                Assert.Equal(typeof(CaptureTrace), messages[4].Payload.GetType());

                conversationId = messages[2].ReadRequiredHeader<ConversationId>().Value;

                trace = await integrationContext
                    .RpcRequest<GetConversationTrace, ConversationTrace>(new GetConversationTrace(conversationId), cts.Token)
                    .ConfigureAwait(false);

                Assert.Equal(conversationId, trace.ConversationId);
                Assert.NotNull(trace.Message);
                Assert.Equal(typeof(Query), trace.Message.ReflectedType);
                Assert.Equal(query.Id, ((Query)trace.Message.Payload).Id);
                Assert.Null(trace.RefuseReason);
                Assert.NotNull(trace.SubsequentTrace);
                Assert.Empty(trace.SubsequentTrace);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task RequestReplyTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            MessagesCollector collector,
            TimeSpan timeout)
        {
            var messageTypes = new[]
            {
                typeof(RequestQueryCommand),
                typeof(Query),
                typeof(Reply)
            };

            var messageHandlerTypes = new[]
            {
                typeof(RequestQueryCommandHandler),
                typeof(QueryAlwaysReplyMessageHandler),
                typeof(ReplyEmptyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(additionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint10))
                .BuildHost();

            var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var command = new RequestQueryCommand(42);

                await host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>()
                    .Send(command, cts.Token)
                    .ConfigureAwait(false);

                await Task.WhenAll(
                        collector.WaitUntilMessageIsNotReceived<Reply>(message => message.Id == command.Id),
                        collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(message => message.HandlerType == typeof(ReplyEmptyMessageHandler)))
                    .ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                Assert.Empty(collector.ErrorMessages);
                var messages = collector.Messages.ToArray();
                Assert.Equal(4, messages.Length);
                Assert.Equal(typeof(RequestQueryCommand), messages[0].Payload.GetType());
                Assert.Equal(typeof(Query), messages[1].Payload.GetType());
                Assert.Equal(typeof(Reply), messages[2].Payload.GetType());
                Assert.Equal(typeof(Endpoint1HandlerInvoked), messages[3].Payload.GetType());
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task RpcRequestTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            MessagesCollector collector,
            TimeSpan timeout)
        {
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

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(additionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint10))
                .BuildHost();

            var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var query = new Query(42);

                var reply = await host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>()
                    .RpcRequest<Query, Reply>(query, cts.Token)
                    .ConfigureAwait(false);

                Assert.Equal(query.Id, reply.Id);

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                Assert.Empty(collector.ErrorMessages);
                Assert.Equal(2, collector.Messages.Count);
                var messages = collector.Messages.ToArray();
                Assert.Equal(typeof(Query), messages[0].Payload.GetType());
                Assert.Equal(typeof(Reply), messages[1].Payload.GetType());
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task EndpointCanHaveSeveralMessageHandlersPerMessage(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            MessagesCollector collector,
            TimeSpan timeout)
        {
            var messageTypes = new[]
            {
                typeof(Command)
            };

            var messageHandlerTypes = new[]
            {
                typeof(CommandEmptyMessageHandler),
                typeof(CommandThrowingMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var overrides = Fixture.DelegateOverride(container =>
            {
                container.Override<IRetryPolicy, DefaultRetryPolicy, RetryPolicyMock>(EnLifestyle.Singleton);
            });

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(additionalOurTypes)
                        .WithOverrides(overrides))
                    .BuildOptions(TestIdentity.Endpoint10))
                .BuildHost();

            var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>()
                    .Send(new Command(42), cts.Token)
                    .ConfigureAwait(false);

                await collector
                    .WaitUntilErrorMessageIsNotReceived<Command>()
                    .ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                Assert.Single(collector.ErrorMessages);
                var (errorMessage, exception) = collector.ErrorMessages.Single();
                Assert.Equal(42.ToString(CultureInfo.InvariantCulture), exception.Message);
                Assert.Equal(3, errorMessage.ReadHeader<RetryCounter>().Value);

                var expectedRetryCounts = new[] { 0, 1, 2, 3 };

                var actualRetryCounts = collector
                    .Messages
                    .Where(message => message.Payload is Command)
                    .Select(message => message.ReadHeader<RetryCounter>()?.Value ?? default(int))
                    .ToList();

                var commandEmptyMessageHandlerInvokesCount = collector
                    .Messages
                    .Count(message => message.Payload is Endpoint1HandlerInvoked handlerInvoked
                                      && handlerInvoked.HandlerType == typeof(CommandEmptyMessageHandler));

                var commandThrowingMessageHandlerInvokesCount = collector
                    .Messages
                    .Count(message => message.Payload is Endpoint1HandlerInvoked handlerInvoked
                                      && handlerInvoked.HandlerType == typeof(CommandThrowingMessageHandler));

                var handlerInvokedCount = collector
                    .Messages
                    .Select(message => message.Payload)
                    .OfType<Endpoint1HandlerInvoked>()
                    .Count();

                Assert.Equal(expectedRetryCounts, actualRetryCounts);
                Assert.Equal(8, collector.Messages.Count);
                Assert.Equal(4, handlerInvokedCount);
                Assert.Equal(4, commandEmptyMessageHandlerInvokesCount);
                Assert.Equal(0, commandThrowingMessageHandlerInvokesCount);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task ContravariantMessageHandlerTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            MessagesCollector collector,
            TimeSpan timeout)
        {
            var messageTypes = new[]
            {
                typeof(PublishInheritedEventCommand),
                typeof(BaseEvent),
                typeof(InheritedEvent)
            };

            var messageHandlerTypes = new[]
            {
                typeof(PublishInheritedEventCommandHandler),
                typeof(IntegrationEventEmptyMessageHandler),
                typeof(BaseEventEmptyMessageHandler),
                typeof(InheritedEventEmptyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(additionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint10))
                .BuildHost();

            var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>()
                    .Send(new PublishInheritedEventCommand(42), cts.Token)
                    .ConfigureAwait(false);

                await Task.WhenAll(
                        collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(message => message.HandlerType == typeof(IntegrationEventEmptyMessageHandler)),
                        collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(message => message.HandlerType == typeof(BaseEventEmptyMessageHandler)),
                        collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(message => message.HandlerType == typeof(InheritedEventEmptyMessageHandler)))
                    .ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                Assert.Empty(collector.ErrorMessages);
                Assert.Equal(5, collector.Messages.Count);
                var messages = collector.Messages.Take(2).ToArray();
                Assert.Equal(typeof(PublishInheritedEventCommand), messages[0].Payload.GetType());
                Assert.Equal(typeof(InheritedEvent), messages[1].Payload.GetType());
                Assert.True(messages.Skip(2).All(message => message.Payload is Endpoint1HandlerInvoked));
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task ThrowingMessageHandlerTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            MessagesCollector collector,
            TimeSpan timeout)
        {
            var endpointIdentity = new EndpointIdentity(TestIdentity.Endpoint1, 0);

            var messageTypes = new[]
            {
                typeof(Command)
            };

            var messageHandlerTypes = new[]
            {
                typeof(CommandThrowingMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var overrides = Fixture.DelegateOverride(container =>
            {
                container.Override<IRetryPolicy, DefaultRetryPolicy, RetryPolicyMock>(EnLifestyle.Singleton);
            });

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithOverrides(overrides)
                        .WithAdditionalOurTypes(additionalOurTypes))
                    .BuildOptions(endpointIdentity))
                .BuildHost();

            var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>()
                    .Send(new Command(42), cts.Token)
                    .ConfigureAwait(false);

                await collector
                    .WaitUntilErrorMessageIsNotReceived<Command>()
                    .ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                Assert.Single(collector.ErrorMessages);
                var (errorMessage, exception) = collector.ErrorMessages.Single();
                Assert.Equal(42.ToString(CultureInfo.InvariantCulture), exception.Message);
                Assert.Equal(3, errorMessage.ReadHeader<RetryCounter>()?.Value);

                var expectedRetryCounters = new[] { 0, 1, 2, 3 };
                var actualRetryCounters = collector
                    .Messages
                    .Select(message => message.ReadHeader<RetryCounter>()?.Value ?? default(int))
                    .ToList();

                Assert.Equal(expectedRetryCounters, actualRetryCounters);

                var actualDeliveries = collector
                    .Messages
                    .Select(message => message.ReadHeader<ActualDeliveryDate>()?.Value ?? default(DateTime))
                    .ToList();

                var latency = 200;

                var expectedDeliveryDelays = new[]
                {
                    0,
                    1000,
                    2000
                };

                var actualDeliveryDelays = actualDeliveries
                    .Zip(actualDeliveries.Skip(1))
                    .Select(period => period.Second - period.First)
                    .Select(span => span.TotalMilliseconds)
                    .ToList();

                Assert.Equal(actualDeliveryDelays.Count, expectedDeliveryDelays.Length);

                Assert.True(actualDeliveryDelays
                    .Zip(expectedDeliveryDelays)
                    .All(delays =>
                    {
                        var actualDelay = (int)delays.First;
                        var expectedDelay = delays.Second;
                        var leftBorder = expectedDelay;
                        var rightBorder = expectedDelay + latency;

                        Output.WriteLine($"{leftBorder} - {actualDelay} - {rightBorder}");
                        return leftBorder <= actualDelay && actualDelay <= rightBorder;
                    }));
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task EventSubscriptionBetweenEndpointsTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            MessagesCollector collector,
            TimeSpan timeout)
        {
            var endpoint1MessageTypes = new[]
            {
                typeof(Event)
            };

            var endpoint1MessageHandlerTypes = new[]
            {
                typeof(EventEmptyMessageHandler)
            };

            var endpoint2MessageTypes = new[]
            {
                typeof(Event),
                typeof(PublishEventCommand)
            };

            var endpoint2MessageHandlerTypes = new[]
            {
                typeof(PublishEventCommandHandler),
                typeof(EventEmptyMessageHandler)
            };

            var endpoint1AdditionalOurTypes = endpoint1MessageTypes.Concat(endpoint1MessageHandlerTypes).ToArray();
            var endpoint2AdditionalOurTypes = endpoint2MessageTypes.Concat(endpoint2MessageHandlerTypes).ToArray();

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(endpoint1AdditionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint10))
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(endpoint1AdditionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint11))
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(endpoint2AdditionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint20))
                .BuildHost();

            var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await host
                    .GetTransportDependencyContainer()
                    .Resolve<IIntegrationContext>()
                    .Send(new PublishEventCommand(42), cts.Token)
                    .ConfigureAwait(false);

                await Task.WhenAll(
                        collector.WaitUntilMessageIsNotReceived<Event>(),
                        collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(message => message.EndpointIdentity.Equals(TestIdentity.Endpoint10) || message.EndpointIdentity.Equals(TestIdentity.Endpoint11)),
                        collector.WaitUntilMessageIsNotReceived<Endpoint2HandlerInvoked>(message => message.EndpointIdentity.Equals(TestIdentity.Endpoint20)))
                    .ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostTestData))]
        internal void SameTransportTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport)
        {
            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithTracing()
                    .BuildOptions(TestIdentity.Endpoint10))
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithTracing()
                    .BuildOptions(TestIdentity.Endpoint20))
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
        internal void BuildHostTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport)
        {
            var endpointIdentity = new EndpointIdentity(TestIdentity.Endpoint1, 0);

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

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithTracing()
                    .ModifyContainerOptions(options => options
                        .WithAdditionalOurTypes(additionalOurTypes))
                    .BuildOptions(endpointIdentity))
                .BuildHost();

            using (host)
            {
                CheckHost(host);
                CheckEndpoint(host, endpointIdentity, Output.WriteLine);
                CheckTransport(host, Output.WriteLine);
            }

            static void CheckHost(IHost host)
            {
                _ = host.Services.GetRequiredService<IHostedService>();

                var expectedHostStartupActions = new[]
                {
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
                var integrationMessage = new IntegrationMessage(new Command(0), typeof(Command), new StringFormatterMock());

                Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage));
                Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<Core.GenericEndpoint.Api.Abstractions.IIntegrationContext>());

                using (endpointDependencyContainer.OpenScope())
                {
                    Assert.Throws<ComponentResolutionException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                    var advancedIntegrationContext = endpointDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage);

                    var expected = new[]
                    {
                        typeof(AdvancedIntegrationContext)
                    };

                    var actual = advancedIntegrationContext
                        .FlattenDecoratedType()
                        .ShowTypes("extended integration context", log)
                        .ToList();

                    Assert.Equal(expected, actual);

                    var expectedPipeline = new[]
                    {
                        typeof(SpaceEngineers.Core.GenericEndpoint.Pipeline.ErrorHandlingPipeline),
                        typeof(SpaceEngineers.Core.GenericEndpoint.Pipeline.UnitOfWorkPipeline),
                        typeof(TracingPipeline),
                        typeof(SpaceEngineers.Core.GenericEndpoint.Pipeline.QueryReplyValidationPipeline),
                        typeof(SpaceEngineers.Core.GenericEndpoint.Pipeline.MessageHandlerPipeline),
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

                var expected = new[]
                {
                    typeof(SpaceEngineers.Core.IntegrationTransport.Integration.IntegrationContext)
                };

                var actual = integrationContext
                    .FlattenDecoratedType()
                    .ShowTypes("transport integration context", log)
                    .ToList();

                Assert.Equal(expected, actual);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task RunTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            MessagesCollector collector,
            TimeSpan timeout)
        {
            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithTracing()
                    .BuildOptions(new EndpointIdentity(nameof(RunTest), 0)))
                .BuildHost();

            var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var runningHost = host.RunAsync(cts.Token);
                await waitUntilTransportIsNotRunning.ConfigureAwait(false);
                await host.StopAsync(cts.Token).ConfigureAwait(false);
                await runningHost.ConfigureAwait(false);

                Assert.Empty(collector.ErrorMessages);
                Assert.Empty(collector.Messages);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task StartStopTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            MessagesCollector collector,
            TimeSpan timeout)
        {
            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithTracing()
                    .BuildOptions(new EndpointIdentity(nameof(StartStopTest), 0)))
                .BuildHost();

            var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);
                await waitUntilTransportIsNotRunning.ConfigureAwait(false);
                await host.StopAsync(cts.Token).ConfigureAwait(false);

                Assert.Empty(collector.ErrorMessages);
                Assert.Empty(collector.Messages);
            }
        }
    }
}