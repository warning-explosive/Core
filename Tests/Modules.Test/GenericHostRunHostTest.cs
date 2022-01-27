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
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Host;
    using GenericEndpoint.Messaging.MessageHeaders;
    using GenericEndpoint.Pipeline;
    using GenericHost;
    using IntegrationTransport.Api.Abstractions;
    using IntegrationTransport.Host;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.Hosting;
    using Mocks;
    using Registrations;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// GenericHost assembly tests
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "reviewed")]
    public class GenericHostRunHostTest : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public GenericHostRunHostTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// useContainer
        /// </summary>
        /// <returns>DependencyContainerImplementations</returns>
        public static IEnumerable<Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>>> DependencyContainerImplementations()
        {
            var dependencyContainerProducers = new[]
            {
                new Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>>(options => options.UseGenericContainer())
            };

            return dependencyContainerProducers;
        }

        /// <summary>
        /// useContainer; useTransport; timeout;
        /// </summary>
        /// <returns>RunHostTestData</returns>
        public static IEnumerable<object[]> RunHostTestData()
        {
            var timeout = TimeSpan.FromSeconds(60);

            var useInMemoryIntegrationTransport =
                new Func<Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>>, Func<IHostBuilder, IHostBuilder>>(
                    useContainer => hostBuilder => hostBuilder
                        .UseIntegrationTransport(builder => builder
                            .WithContainer(useContainer)
                            .WithInMemoryIntegrationTransport(hostBuilder)
                            .WithDefaultCrossCuttingConcerns()
                            .ModifyContainerOptions(options => options.WithManualRegistrations(new MessagesCollectorManualRegistration()))
                            .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                useInMemoryIntegrationTransport
            };

            return DependencyContainerImplementations()
                .SelectMany(useContainer => integrationTransportProviders
                    .Select(useTransport => new object[]
                        {
                            useContainer,
                            useTransport(useContainer),
                            timeout
                        }));
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task RequestReplyTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
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
                .UseEndpoint(
                    TestIdentity.Endpoint10,
                    (_, builder) => builder
                        .WithContainer(useContainer)
                        .WithDefaultCrossCuttingConcerns()
                        .ModifyContainerOptions(options => options.WithAdditionalOurTypes(additionalOurTypes))
                        .BuildOptions())
                .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();
            var collector = transportDependencyContainer.Resolve<MessagesCollector>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var command = new RequestQueryCommand(42);

                await integrationContext
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
                Assert.Equal(typeof(RequestQueryCommand), messages[0].ReflectedType);
                Assert.Equal(typeof(Query), messages[1].ReflectedType);
                Assert.Equal(typeof(Reply), messages[2].ReflectedType);
                Assert.Equal(typeof(Endpoint1HandlerInvoked), messages[3].ReflectedType);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task RpcRequestTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
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
                .UseEndpoint(
                    TestIdentity.Endpoint10,
                    (_, builder) => builder
                        .WithContainer(useContainer)
                        .WithDefaultCrossCuttingConcerns()
                        .ModifyContainerOptions(options => options.WithAdditionalOurTypes(additionalOurTypes))
                        .BuildOptions())
                .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();
            var collector = transportDependencyContainer.Resolve<MessagesCollector>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var query = new Query(42);

                var reply = await integrationContext
                    .RpcRequest<Query, Reply>(query, cts.Token)
                    .ConfigureAwait(false);

                Assert.Equal(query.Id, reply.Id);

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                Assert.Empty(collector.ErrorMessages);
                Assert.Equal(2, collector.Messages.Count);
                var messages = collector.Messages.ToArray();
                Assert.Equal(typeof(Query), messages[0].ReflectedType);
                Assert.Equal(typeof(Reply), messages[1].ReflectedType);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task EndpointCanHaveSeveralMessageHandlersPerMessage(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
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
                container.Override<IRetryPolicy, RetryPolicyMock>(EnLifestyle.Singleton);
            });

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseEndpoint(
                    TestIdentity.Endpoint10,
                    (_, builder) => builder
                        .WithContainer(useContainer)
                        .WithDefaultCrossCuttingConcerns()
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(additionalOurTypes)
                            .WithOverrides(overrides))
                        .BuildOptions())
                .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();
            var collector = transportDependencyContainer.Resolve<MessagesCollector>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await integrationContext
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
                .UseEndpoint(
                    TestIdentity.Endpoint10,
                    (_, builder) => builder
                        .WithContainer(useContainer)
                        .WithDefaultCrossCuttingConcerns()
                        .ModifyContainerOptions(options => options.WithAdditionalOurTypes(additionalOurTypes))
                        .BuildOptions())
                .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();
            var collector = transportDependencyContainer.Resolve<MessagesCollector>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await integrationContext
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
                Assert.Equal(typeof(PublishInheritedEventCommand), messages[0].ReflectedType);
                Assert.Equal(typeof(InheritedEvent), messages[1].ReflectedType);
                Assert.True(messages.Skip(2).All(message => message.Payload is Endpoint1HandlerInvoked));
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task ThrowingMessageHandlerTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
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
                container.Override<IRetryPolicy, RetryPolicyMock>(EnLifestyle.Singleton);
            });

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseEndpoint(
                    TestIdentity.Endpoint10,
                    (_, builder) => builder
                        .WithContainer(useContainer)
                        .WithDefaultCrossCuttingConcerns()
                        .ModifyContainerOptions(options => options
                            .WithOverrides(overrides)
                            .WithAdditionalOurTypes(additionalOurTypes))
                        .BuildOptions())
                .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();
            var collector = transportDependencyContainer.Resolve<MessagesCollector>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await integrationContext
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
                .UseEndpoint(
                    TestIdentity.Endpoint10,
                    (_, builder) => builder
                        .WithContainer(useContainer)
                        .WithDefaultCrossCuttingConcerns()
                        .ModifyContainerOptions(options => options.WithAdditionalOurTypes(endpoint1AdditionalOurTypes))
                        .BuildOptions())
                .UseEndpoint(
                    TestIdentity.Endpoint11,
                    (_, builder) => builder
                        .WithContainer(useContainer)
                        .WithDefaultCrossCuttingConcerns()
                        .ModifyContainerOptions(options => options.WithAdditionalOurTypes(endpoint1AdditionalOurTypes))
                        .BuildOptions())
                .UseEndpoint(
                    TestIdentity.Endpoint20,
                    (_, builder) => builder
                        .WithContainer(useContainer)
                        .WithDefaultCrossCuttingConcerns()
                        .ModifyContainerOptions(options => options.WithAdditionalOurTypes(endpoint2AdditionalOurTypes))
                        .BuildOptions())
                .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();
            var collector = transportDependencyContainer.Resolve<MessagesCollector>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await integrationContext
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
        [MemberData(nameof(RunHostTestData))]
        internal async Task RunTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            var host = useTransport(Host.CreateDefaultBuilder())
                .UseEndpoint(
                    new EndpointIdentity(nameof(RunTest), 0),
                    (_, builder) => builder
                        .WithContainer(useContainer)
                        .WithDefaultCrossCuttingConcerns()
                        .WithTracing()
                        .BuildOptions())
                .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var collector = transportDependencyContainer.Resolve<MessagesCollector>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

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
            TimeSpan timeout)
        {
            var host = useTransport(Host.CreateDefaultBuilder())
                .UseEndpoint(
                    new EndpointIdentity(nameof(StartStopTest), 0),
                    (_, builder) => builder
                        .WithContainer(useContainer)
                        .WithDefaultCrossCuttingConcerns()
                        .WithTracing()
                        .BuildOptions())
                .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var collector = transportDependencyContainer.Resolve<MessagesCollector>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                Assert.Empty(collector.ErrorMessages);
                Assert.Empty(collector.Messages);
            }
        }
    }
}