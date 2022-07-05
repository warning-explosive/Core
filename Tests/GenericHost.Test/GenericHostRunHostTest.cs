namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CompositionRoot.Api.Abstractions.Registration;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Host;
    using GenericEndpoint.Messaging.MessageHeaders;
    using GenericEndpoint.Pipeline;
    using GenericHost;
    using IntegrationTransport.Host;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Mocks;
    using Overrides;
    using Registrations;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// GenericHost assembly tests
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
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
        /// useContainer; useTransport; timeout;
        /// </summary>
        /// <returns>RunHostTestData</returns>
        public static IEnumerable<object[]> RunHostTestData()
        {
            var timeout = TimeSpan.FromSeconds(60);

            var useInMemoryIntegrationTransport = new Func<string, ILogger, IHostBuilder, IHostBuilder>(
                (test, logger, hostBuilder) => hostBuilder
                   .UseIntegrationTransport(builder => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .ModifyContainerOptions(options => options
                           .WithManualRegistrations(new MessagesCollectorManualRegistration())
                           .WithManualRegistrations(new AnonymousUserScopeProviderManualRegistration())
                           .WithOverrides(new TestLoggerOverride(logger))
                           .WithOverrides(new TestSettingsScopeProviderOverride(test)))
                       .BuildOptions()));

            var useRabbitMqIntegrationTransport = new Func<string, ILogger, IHostBuilder, IHostBuilder>(
                (test, logger, hostBuilder) => hostBuilder
                   .UseIntegrationTransport(builder => builder
                       .WithRabbitMqIntegrationTransport(hostBuilder)
                       .ModifyContainerOptions(options => options
                           .WithManualRegistrations(new MessagesCollectorManualRegistration())
                           .WithManualRegistrations(new AnonymousUserScopeProviderManualRegistration())
                           .WithOverrides(new TestLoggerOverride(logger))
                           .WithOverrides(new TestSettingsScopeProviderOverride(test)))
                       .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                useInMemoryIntegrationTransport,
                useRabbitMqIntegrationTransport
            };

            return integrationTransportProviders
               .Select(useTransport => new object[]
               {
                   useTransport,
                   timeout
               });
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task RequestReplyTest(
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            var logger = Fixture.CreateLogger(Output);

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

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(nameof(RequestReplyTest))
            };

            var host = useTransport(
                    nameof(RequestReplyTest),
                    logger,
                    Fixture.CreateHostBuilder(Output))
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithOverrides(overrides))
                       .BuildOptions())
               .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var hostShutdown = host.WaitForShutdownAsync(cts.Token);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var command = new RequestQueryCommand(42);

                var awaiter = Task.WhenAny(
                    hostShutdown,
                    Task.WhenAll(
                        collector.WaitUntilMessageIsNotReceived<Reply>(message => message.Id == command.Id),
                        collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(message => message.HandlerType == typeof(ReplyEmptyMessageHandler))));

                await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                    await integrationContext
                       .Send(command, cts.Token)
                       .ConfigureAwait(false);
                }

                if (hostShutdown == await awaiter.ConfigureAwait(false))
                {
                    throw new InvalidOperationException("Host was unexpectedly stopped");
                }

                var errorMessages = collector.ErrorMessages.ToArray();
                Assert.Single(errorMessages);
                Assert.Single(errorMessages.Where(info => info.message.ReflectedType == typeof(Endpoint1HandlerInvoked) && ((Endpoint1HandlerInvoked)info.message.Payload).HandlerType == typeof(ReplyEmptyMessageHandler)));

                var messages = collector.Messages.ToArray();
                Assert.Equal(3, messages.Length);
                Assert.Single(messages.Where(message => message.ReflectedType == typeof(RequestQueryCommand)));
                Assert.Single(messages.Where(message => message.ReflectedType == typeof(Query)));
                Assert.Single(messages.Where(message => message.ReflectedType == typeof(Reply)));

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task RpcRequestTest(
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
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

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(nameof(RpcRequestTest))
            };

            var host = useTransport(
                    nameof(RpcRequestTest),
                    logger,
                    Fixture.CreateHostBuilder(Output))
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithOverrides(overrides))
                       .BuildOptions())
               .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var hostShutdown = host.WaitForShutdownAsync(cts.Token);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var query = new Query(42);

                Reply reply;

                await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                    var awaiter = Task.WhenAny(
                        hostShutdown,
                        integrationContext.RpcRequest<Query, Reply>(query, cts.Token));

                    var result = await awaiter.ConfigureAwait(false);

                    if (hostShutdown == result)
                    {
                        throw new InvalidOperationException("Host was unexpectedly stopped");
                    }

                    reply = await ((Task<Reply>)result).ConfigureAwait(false);
                }

                Assert.Empty(collector.ErrorMessages);

                var messages = collector.Messages.ToArray();
                Assert.Equal(2, messages.Length);
                Assert.Single(messages.Where(message => message.ReflectedType == typeof(Query)));
                Assert.Single(messages.Where(message => message.ReflectedType == typeof(Reply)));
                Assert.Equal(query.Id, reply.Id);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task ContravariantMessageHandlerTest(
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            var logger = Fixture.CreateLogger(Output);

            var messageTypes = new[]
            {
                typeof(PublishInheritedEventCommand),
                typeof(BaseEvent),
                typeof(InheritedEvent)
            };

            var messageHandlerTypes = new[]
            {
                typeof(PublishInheritedEventCommandHandler),
                typeof(BaseEventEmptyMessageHandler),
                typeof(InheritedEventEmptyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(nameof(ContravariantMessageHandlerTest))
            };

            var host = useTransport(
                    nameof(ContravariantMessageHandlerTest),
                    logger,
                    Fixture.CreateHostBuilder(Output))
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithOverrides(overrides))
                       .BuildOptions())
               .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var hostShutdown = host.WaitForShutdownAsync(cts.Token);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var awaiter = Task.WhenAny(
                        hostShutdown,
                        Task.WhenAll(
                            collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(message => message.HandlerType == typeof(BaseEventEmptyMessageHandler)),
                            collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(message => message.HandlerType == typeof(InheritedEventEmptyMessageHandler))));

                await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                    await integrationContext
                       .Send(new PublishInheritedEventCommand(42), cts.Token)
                       .ConfigureAwait(false);
                }

                if (hostShutdown == await awaiter.ConfigureAwait(false))
                {
                    throw new InvalidOperationException("Host was unexpectedly stopped");
                }

                var errorMessages = collector.ErrorMessages.ToArray();
                Assert.Equal(2, errorMessages.Length);
                Assert.Single(errorMessages.Where(info => info.message.ReflectedType == typeof(Endpoint1HandlerInvoked) && ((Endpoint1HandlerInvoked)info.message.Payload).HandlerType == typeof(BaseEventEmptyMessageHandler)));
                Assert.Single(errorMessages.Where(info => info.message.ReflectedType == typeof(Endpoint1HandlerInvoked) && ((Endpoint1HandlerInvoked)info.message.Payload).HandlerType == typeof(InheritedEventEmptyMessageHandler)));

                var messages = collector.Messages.ToArray();
                Assert.Equal(3, messages.Length);
                Assert.Single(messages.Where(message => message.ReflectedType == typeof(PublishInheritedEventCommand)));
                Assert.Single(messages.Where(message => message.Payload.GetType() == typeof(InheritedEvent) && message.ReflectedType == typeof(BaseEvent)));
                Assert.Single(messages.Where(message => message.Payload.GetType() == typeof(InheritedEvent) && message.ReflectedType == typeof(InheritedEvent)));

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task ThrowingMessageHandlerTest(
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            var logger = Fixture.CreateLogger(Output);

            var messageTypes = new[]
            {
                typeof(Command)
            };

            var messageHandlerTypes = new[]
            {
                typeof(CommandThrowingMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(nameof(ThrowingMessageHandlerTest)),
                Fixture.DelegateOverride(container =>
                {
                    container.Override<IRetryPolicy, TestRetryPolicy>(EnLifestyle.Singleton);
                })
            };

            var host = useTransport(nameof(ThrowingMessageHandlerTest), logger, Fixture.CreateHostBuilder(Output))
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithOverrides(overrides))
                       .BuildOptions())
               .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var hostShutdown = host.WaitForShutdownAsync(cts.Token);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                    await integrationContext
                       .Send(new Command(42), cts.Token)
                       .ConfigureAwait(false);
                }

                var awaiter = Task.WhenAny(
                    hostShutdown,
                    collector.WaitUntilErrorMessageIsNotReceived<Command>());

                if (hostShutdown == await awaiter.ConfigureAwait(false))
                {
                    throw new InvalidOperationException("Host was unexpectedly stopped");
                }

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
                    .Select(message => message.ReadRequiredHeader<ActualDeliveryDate>().Value)
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

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task EventSubscriptionBetweenEndpointsTest(
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            var logger = Fixture.CreateLogger(Output);

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

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(nameof(EventSubscriptionBetweenEndpointsTest))
            };

            var host = useTransport(
                    nameof(EventSubscriptionBetweenEndpointsTest),
                    logger,
                    Fixture.CreateHostBuilder(Output))
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(endpoint1AdditionalOurTypes)
                           .WithOverrides(overrides))
                       .BuildOptions())
               .UseEndpoint(TestIdentity.Endpoint11,
                    (_, builder) => builder
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(endpoint1AdditionalOurTypes)
                           .WithOverrides(overrides))
                       .BuildOptions())
               .UseEndpoint(TestIdentity.Endpoint20,
                    (_, builder) => builder
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(endpoint2AdditionalOurTypes)
                           .WithOverrides(overrides))
                       .BuildOptions())
               .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var hostShutdown = host.WaitForShutdownAsync(cts.Token);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                    await integrationContext
                       .Send(new PublishEventCommand(42), cts.Token)
                       .ConfigureAwait(false);
                }

                var awaiter = Task.WhenAny(
                    hostShutdown,
                    Task.WhenAll(
                        collector.WaitUntilMessageIsNotReceived<Event>(),
                        collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(message => message.EndpointIdentity.Equals(TestIdentity.Endpoint10) || message.EndpointIdentity.Equals(TestIdentity.Endpoint11)),
                        collector.WaitUntilMessageIsNotReceived<Endpoint2HandlerInvoked>(message => message.EndpointIdentity.Equals(TestIdentity.Endpoint20))));

                if (hostShutdown == await awaiter.ConfigureAwait(false))
                {
                    throw new InvalidOperationException("Host was unexpectedly stopped");
                }

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task RunTest(
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            var logger = Fixture.CreateLogger(Output);

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(nameof(RunTest))
            };

            var host = useTransport(
                    nameof(RunTest),
                    logger,
                    Fixture.CreateHostBuilder(Output))
               .UseEndpoint(new EndpointIdentity(nameof(RunTest), 0),
                    (_, builder) => builder
                       .WithTracing()
                       .ModifyContainerOptions(options => options
                           .WithOverrides(overrides))
                       .BuildOptions())
               .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                var runningHost = host.RunAsync(cts.Token);

                var hostShutdown = host.WaitForShutdownAsync(cts.Token);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                await runningHost.ConfigureAwait(false);
                await hostShutdown.ConfigureAwait(false);

                Assert.Empty(collector.ErrorMessages);
                Assert.Empty(collector.Messages);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task StartStopTest(
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            var logger = Fixture.CreateLogger(Output);

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(nameof(StartStopTest))
            };

            var host = useTransport(
                    nameof(StartStopTest),
                    logger,
                    Fixture.CreateHostBuilder(Output))
               .UseEndpoint(new EndpointIdentity(nameof(StartStopTest), 0),
                    (_, builder) => builder
                       .WithTracing()
                       .ModifyContainerOptions(options => options
                           .WithOverrides(overrides))
                       .BuildOptions())
               .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var hostShutdown = host.WaitForShutdownAsync(cts.Token);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                await hostShutdown.ConfigureAwait(false);

                Assert.Empty(collector.ErrorMessages);
                Assert.Empty(collector.Messages);
            }
        }
    }
}