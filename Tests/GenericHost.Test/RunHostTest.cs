namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using CompositionRoot.Registration;
    using CrossCuttingConcerns.Settings;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Host;
    using GenericEndpoint.Messaging.MessageHeaders;
    using GenericEndpoint.Pipeline;
    using GenericHost;
    using IntegrationTransport.Host;
    using IntegrationTransport.RabbitMQ.Settings;
    using IntegrationTransport.RpcRequest;
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
    /// RunHostTest
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
    public class RunHostTest : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public RunHostTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// RunHostTestData
        /// </summary>
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> RunHostTestData()
        {
            var timeout = TimeSpan.FromSeconds(60);

            var settingsDirectory = SolutionExtensions
               .ProjectFile()
               .Directory
               .EnsureNotNull("Project directory wasn't found")
               .StepInto("Settings");

            var useInMemoryIntegrationTransport = new Func<string, ILogger, IHostBuilder, IHostBuilder>(
                (settingsScope, logger, hostBuilder) => hostBuilder
                   .UseIntegrationTransport(builder => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .ModifyContainerOptions(options => options
                           .WithManualRegistrations(new MessagesCollectorManualRegistration())
                           .WithManualRegistrations(new VirtualHostManualRegistration(settingsScope))
                           .WithOverrides(new TestLoggerOverride(logger))
                           .WithOverrides(new TestSettingsScopeProviderOverride(settingsScope)))
                       .BuildOptions()));

            var useRabbitMqIntegrationTransport = new Func<string, ILogger, IHostBuilder, IHostBuilder>(
                (settingsScope, logger, hostBuilder) => hostBuilder
                   .UseIntegrationTransport(builder => builder
                       .WithRabbitMqIntegrationTransport(hostBuilder)
                       .ModifyContainerOptions(options => options
                           .WithManualRegistrations(new MessagesCollectorManualRegistration())
                           .WithManualRegistrations(new VirtualHostManualRegistration(settingsScope))
                           .WithOverrides(new TestLoggerOverride(logger))
                           .WithOverrides(new TestSettingsScopeProviderOverride(settingsScope)))
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
                   useTransport,
                   timeout
               });
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task RequestReplyTest(
            DirectoryInfo settingsDirectory,
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            var logger = Fixture.CreateLogger(Output);

            var settingsScope = nameof(RequestReplyTest);

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
                new TestSettingsScopeProviderOverride(settingsScope)
            };

            var host = useTransport(
                    settingsScope,
                    logger,
                    Fixture.CreateHostBuilder(Output))
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithOverrides(overrides))
                       .BuildOptions())
               .BuildHost(settingsDirectory);

            await RunTestHost(Output, host, RequestReplyTestInternal(settingsScope), timeout).ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> RequestReplyTestInternal(string settingsScope)
            {
                return async (_, host, token) =>
                {
                    var transportDependencyContainer = host.GetTransportDependencyContainer();
                    var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                    var rabbitMqSettings = await transportDependencyContainer
                       .Resolve<ISettingsProvider<RabbitMqSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(settingsScope, rabbitMqSettings.VirtualHost);

                    var command = new RequestQueryCommand(42);

                    var awaiter = Task.WhenAll(
                        collector.WaitUntilMessageIsNotReceived<Reply>(message => message.Id == command.Id),
                        collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(message => message.HandlerType == typeof(ReplyEmptyMessageHandler)));

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                        await integrationContext
                           .Send(command, token)
                           .ConfigureAwait(false);
                    }

                    await awaiter.ConfigureAwait(false);

                    var errorMessages = collector.ErrorMessages.ToArray();
                    Assert.Single(errorMessages);
                    Assert.Single(errorMessages.Where(info => info.message.ReflectedType == typeof(Endpoint1HandlerInvoked) && ((Endpoint1HandlerInvoked)info.message.Payload).HandlerType == typeof(ReplyEmptyMessageHandler)));

                    var messages = collector.Messages.ToArray();
                    Assert.Equal(3, messages.Length);
                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(RequestQueryCommand)));
                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(Query)));
                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(Reply)));
                };
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task RpcRequestTest(
            DirectoryInfo settingsDirectory,
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            var logger = Fixture.CreateLogger(Output);

            var settingsScope = nameof(RpcRequestTest);

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
                new TestSettingsScopeProviderOverride(settingsScope)
            };

            var host = useTransport(
                    settingsScope,
                    logger,
                    Fixture.CreateHostBuilder(Output))
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithOverrides(overrides))
                       .BuildOptions())
               .BuildHost(settingsDirectory);

            await RunTestHost(Output, host, RpcRequestTestInternal(settingsScope), timeout).ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> RpcRequestTestInternal(string settingsScope)
            {
                return async (_, host, token) =>
                {
                    var transportDependencyContainer = host.GetTransportDependencyContainer();

                    var rabbitMqSettings = await transportDependencyContainer
                       .Resolve<ISettingsProvider<RabbitMqSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(settingsScope, rabbitMqSettings.VirtualHost);

                    var query = new Query(42);

                    Reply reply;

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                        reply = await integrationContext
                           .RpcRequest<Query, Reply>(query, token)
                           .ConfigureAwait(false);
                    }

                    Assert.Equal(query.Id, reply.Id);

                    var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();
                    Assert.Empty(collector.ErrorMessages);

                    var messages = collector.Messages.ToArray();
                    Assert.Equal(2, messages.Length);

                    var integrationQuery = messages.Where(message => message.ReflectedType == typeof(Query) && message.ReadRequiredHeader<SentFrom>().Value.LogicalName.Equals(TransportEndpointIdentity.LogicalName, StringComparison.OrdinalIgnoreCase)).ToArray();
                    Assert.Single(integrationQuery);
                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(Reply) && message.ReadRequiredHeader<InitiatorMessageId>().Value.Equals(integrationQuery.Single().ReadRequiredHeader<Id>().Value)));
                };
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task ContravariantMessageHandlerTest(
            DirectoryInfo settingsDirectory,
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            var logger = Fixture.CreateLogger(Output);

            var settingsScope = nameof(ContravariantMessageHandlerTest);

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
                new TestSettingsScopeProviderOverride(settingsScope)
            };

            var host = useTransport(
                    settingsScope,
                    logger,
                    Fixture.CreateHostBuilder(Output))
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithOverrides(overrides))
                       .BuildOptions())
               .BuildHost(settingsDirectory);

            await RunTestHost(Output, host, ContravariantMessageHandlerTestInternal(settingsScope), timeout).ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> ContravariantMessageHandlerTestInternal(string settingsScope)
            {
                return async (_, host, token) =>
                {
                    var transportDependencyContainer = host.GetTransportDependencyContainer();
                    var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                    var rabbitMqSettings = await transportDependencyContainer
                       .Resolve<ISettingsProvider<RabbitMqSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(settingsScope, rabbitMqSettings.VirtualHost);

                    var awaiter = Task.WhenAll(
                        collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(message => message.HandlerType == typeof(BaseEventEmptyMessageHandler)),
                        collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(message => message.HandlerType == typeof(InheritedEventEmptyMessageHandler)));

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                        await integrationContext
                           .Send(new PublishInheritedEventCommand(42), token)
                           .ConfigureAwait(false);
                    }

                    await awaiter.ConfigureAwait(false);

                    var errorMessages = collector.ErrorMessages.ToArray();
                    Assert.Equal(2, errorMessages.Length);
                    Assert.Single(errorMessages.Where(info => info.message.ReflectedType == typeof(Endpoint1HandlerInvoked) && ((Endpoint1HandlerInvoked)info.message.Payload).HandlerType == typeof(BaseEventEmptyMessageHandler)));
                    Assert.Single(errorMessages.Where(info => info.message.ReflectedType == typeof(Endpoint1HandlerInvoked) && ((Endpoint1HandlerInvoked)info.message.Payload).HandlerType == typeof(InheritedEventEmptyMessageHandler)));

                    var messages = collector.Messages.ToArray();
                    Assert.Equal(3, messages.Length);
                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(PublishInheritedEventCommand)));
                    Assert.Single(messages.Where(message => message.Payload.GetType() == typeof(InheritedEvent) && message.ReflectedType == typeof(BaseEvent)));
                    Assert.Single(messages.Where(message => message.Payload.GetType() == typeof(InheritedEvent) && message.ReflectedType == typeof(InheritedEvent)));
                };
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task ThrowingMessageHandlerTest(
            DirectoryInfo settingsDirectory,
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            var logger = Fixture.CreateLogger(Output);

            var settingsScope = nameof(ThrowingMessageHandlerTest);

            var messageTypes = new[]
            {
                typeof(Command)
            };

            var messageHandlerTypes = new[]
            {
                typeof(CommandThrowingMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var overrides = new[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(settingsScope),
                Fixture.DelegateOverride(container =>
                {
                    container.Override<IRetryPolicy, TestRetryPolicy>(EnLifestyle.Singleton);
                })
            };

            var host = useTransport(
                    settingsScope,
                    logger,
                    Fixture.CreateHostBuilder(Output))
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithOverrides(overrides))
                       .BuildOptions())
               .BuildHost(settingsDirectory);

            await RunTestHost(Output, host, ThrowingMessageHandlerTestInternal(settingsScope), timeout).ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> ThrowingMessageHandlerTestInternal(string settingsScope)
            {
                return async (output, host, token) =>
                {
                    var transportDependencyContainer = host.GetTransportDependencyContainer();
                    var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                    var rabbitMqSettings = await transportDependencyContainer
                       .Resolve<ISettingsProvider<RabbitMqSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(settingsScope, rabbitMqSettings.VirtualHost);

                    var awaiter = collector.WaitUntilErrorMessageIsNotReceived<Command>();

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                        await integrationContext
                           .Send(new Command(42), token)
                           .ConfigureAwait(false);
                    }

                    await awaiter.ConfigureAwait(false);

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
                        .Zip(expectedDeliveryDelays, (actual, expected) => ((int)actual, expected))
                        .All(delays =>
                        {
                            var (actual, expected) = delays;
                            output.WriteLine($"{0.95 * expected} ({0.95} * {expected}) <= {actual}");
                            return 0.95 * expected <= actual;
                        }));
                };
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task EventSubscriptionBetweenEndpointsTest(
            DirectoryInfo settingsDirectory,
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            var logger = Fixture.CreateLogger(Output);

            var settingsScope = nameof(EventSubscriptionBetweenEndpointsTest);

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
                new TestSettingsScopeProviderOverride(settingsScope)
            };

            var host = useTransport(
                    settingsScope,
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
               .BuildHost(settingsDirectory);

            await RunTestHost(Output, host, EventSubscriptionBetweenEndpointsTestInternal(settingsScope), timeout).ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> EventSubscriptionBetweenEndpointsTestInternal(string settingsScope)
            {
                return async (_, host, token) =>
                {
                    var transportDependencyContainer = host.GetTransportDependencyContainer();
                    var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                    var rabbitMqSettings = await transportDependencyContainer
                       .Resolve<ISettingsProvider<RabbitMqSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(settingsScope, rabbitMqSettings.VirtualHost);

                    var awaiter = Task.WhenAll(
                        collector.WaitUntilMessageIsNotReceived<Event>(),
                        collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(message => message.EndpointIdentity.Equals(TestIdentity.Endpoint10) || message.EndpointIdentity.Equals(TestIdentity.Endpoint11)),
                        collector.WaitUntilMessageIsNotReceived<Endpoint2HandlerInvoked>(message => message.EndpointIdentity.Equals(TestIdentity.Endpoint20)));

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                        await integrationContext
                           .Send(new PublishEventCommand(42), token)
                           .ConfigureAwait(false);
                    }

                    await awaiter.ConfigureAwait(false);
                };
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task StartStopTest(
            DirectoryInfo settingsDirectory,
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            var logger = Fixture.CreateLogger(Output);

            var settingsScope = nameof(StartStopTest);

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(settingsScope)
            };

            var host = useTransport(
                    settingsScope,
                    logger,
                    Fixture.CreateHostBuilder(Output))
               .UseEndpoint(new EndpointIdentity(settingsScope, Guid.NewGuid().ToString()),
                    (_, builder) => builder
                       .ModifyContainerOptions(options => options
                           .WithOverrides(overrides))
                       .BuildOptions())
               .BuildHost(settingsDirectory);

            await RunTestHost(Output, host, StartStopTestInternal(settingsScope), timeout).ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> StartStopTestInternal(string settingsScope)
            {
                return async (_, host, token) =>
                {
                    var transportDependencyContainer = host.GetTransportDependencyContainer();
                    var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                    var rabbitMqSettings = await transportDependencyContainer
                       .Resolve<ISettingsProvider<RabbitMqSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(settingsScope, rabbitMqSettings.VirtualHost);

                    Assert.Empty(collector.ErrorMessages);
                    Assert.Empty(collector.Messages);
                };
            }
        }

        internal static async Task RunTestHost(
            ITestOutputHelper output,
            IHost host,
            Func<ITestOutputHelper, IHost, CancellationToken, Task> producer,
            TimeSpan timeout)
        {
            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var hostShutdown = host.WaitForShutdownAsync(cts.Token);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var awaiter = Task.WhenAny(producer(output, host, cts.Token), hostShutdown);

                var result = await awaiter.ConfigureAwait(false);

                if (hostShutdown == result)
                {
                    throw new InvalidOperationException("Host was unexpectedly stopped");
                }

                await result.ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                await hostShutdown.ConfigureAwait(false);
            }
        }
    }
}