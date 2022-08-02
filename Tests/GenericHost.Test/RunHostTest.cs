﻿namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using CompositionRoot.Api.Abstractions.Registration;
    using CrossCuttingConcerns.Settings;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Host;
    using GenericEndpoint.Messaging.MessageHeaders;
    using GenericEndpoint.Pipeline;
    using GenericHost;
    using IntegrationTransport.Api.Abstractions;
    using IntegrationTransport.Host;
    using IntegrationTransport.InMemory;
    using IntegrationTransport.RabbitMQ.Settings;
    using IntegrationTransport.RpcRequest;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.Configuration;
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
        /// <param name="fixture">ModulesTestFixture</param>
        public RunHostTest(ITestOutputHelper output, ModulesTestFixture fixture)
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

            var commonAppSettingsJson = SolutionExtensions
               .ProjectFile()
               .Directory
               .EnsureNotNull("Project directory not found")
               .StepInto("Settings")
               .GetFile("appsettings", ".json")
               .FullName;

            var useInMemoryIntegrationTransport = new Func<EndpointIdentity, string, ILogger, IHostBuilder, Func<DependencyContainerOptions, DependencyContainerOptions>, IHostBuilder>(
                (transportEndpointIdentity, settingsScope, logger, hostBuilder, modifier) => hostBuilder
                   .ConfigureAppConfiguration(builder => builder.AddJsonFile(commonAppSettingsJson))
                   .UseIntegrationTransport(transportEndpointIdentity,
                        builder => builder
                           .WithInMemoryIntegrationTransport(hostBuilder)
                           .ModifyContainerOptions(options => options
                               .WithManualRegistrations(new MessagesCollectorManualRegistration())
                               .WithManualRegistrations(new AnonymousUserScopeProviderManualRegistration())
                               .WithManualRegistrations(new VirtualHostManualRegistration(settingsScope))
                               .WithOverrides(new TestLoggerOverride(logger))
                               .WithOverrides(new TestSettingsScopeProviderOverride(settingsScope)))
                           .ModifyContainerOptions(modifier)
                           .BuildOptions()));

            var useRabbitMqIntegrationTransport = new Func<EndpointIdentity, string, ILogger, IHostBuilder, Func<DependencyContainerOptions, DependencyContainerOptions>, IHostBuilder>(
                (transportEndpointIdentity, settingsScope, logger, hostBuilder, modifier) => hostBuilder
                   .ConfigureAppConfiguration(builder => builder.AddJsonFile(commonAppSettingsJson))
                   .UseIntegrationTransport(transportEndpointIdentity,
                        builder => builder
                           .WithRabbitMqIntegrationTransport(hostBuilder)
                           .ModifyContainerOptions(options => options
                               .WithManualRegistrations(new MessagesCollectorManualRegistration())
                               .WithManualRegistrations(new AnonymousUserScopeProviderManualRegistration())
                               .WithManualRegistrations(new VirtualHostManualRegistration(settingsScope))
                               .WithOverrides(new TestLoggerOverride(logger))
                               .WithOverrides(new TestSettingsScopeProviderOverride(settingsScope)))
                           .ModifyContainerOptions(modifier)
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
            Func<EndpointIdentity, string, ILogger, IHostBuilder, Func<DependencyContainerOptions, DependencyContainerOptions>, IHostBuilder> useTransport,
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
                    new EndpointIdentity(TransportEndpointIdentity.LogicalName, Guid.NewGuid()),
                    settingsScope,
                    logger,
                    Fixture.CreateHostBuilder(Output),
                    options => options)
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
                var rabbitMqSettings = await transportDependencyContainer
                   .Resolve<ISettingsProvider<RabbitMqSettings>>()
                   .Get(cts.Token)
                   .ConfigureAwait(false);

                Assert.Equal(settingsScope, rabbitMqSettings.VirtualHost);

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

                var result = await awaiter.ConfigureAwait(false);

                if (hostShutdown == result)
                {
                    throw new InvalidOperationException("Host was unexpectedly stopped");
                }

                await result.ConfigureAwait(false);

                var errorMessages = collector.ErrorMessages.ToArray();
                Assert.Single(errorMessages);
                Assert.Single(errorMessages.Where(info => info.message.ReflectedType == typeof(Endpoint1HandlerInvoked) && ((Endpoint1HandlerInvoked)info.message.Payload).HandlerType == typeof(ReplyEmptyMessageHandler)));

                var messages = collector.Messages.ToArray();
                Assert.Equal(3, messages.Length);
                Assert.Single(messages.Where(message => message.ReflectedType == typeof(RequestQueryCommand)));
                Assert.Single(messages.Where(message => message.ReflectedType == typeof(Query)));
                Assert.Single(messages.Where(message => message.ReflectedType == typeof(Reply)));

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                await hostShutdown.ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task RpcRequestTest(
            Func<EndpointIdentity, string, ILogger, IHostBuilder, Func<DependencyContainerOptions, DependencyContainerOptions>, IHostBuilder> useTransport,
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

            var transportEndpointIdentity = new EndpointIdentity(TransportEndpointIdentity.LogicalName, Guid.NewGuid());

            var host = useTransport(
                    transportEndpointIdentity,
                    settingsScope,
                    logger,
                    Fixture.CreateHostBuilder(Output),
                    options => options)
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithOverrides(overrides))
                       .BuildOptions())
               .BuildHost();

            var gatewayTransportEndpointIdentity = new EndpointIdentity("Gateway" + TransportEndpointIdentity.LogicalName, Guid.NewGuid());

            var gatewayHost = useTransport(
                    gatewayTransportEndpointIdentity,
                    settingsScope,
                    logger,
                    Fixture.CreateHostBuilder(Output),
                    options =>
                    {
                        var integrationTransport = host
                           .GetTransportDependencyContainer()
                           .Resolve<IIntegrationTransport>();

                        return integrationTransport is InMemoryIntegrationTransport
                            ? options.WithOverrides(Fixture.DelegateOverride(container => container.OverrideInstance(integrationTransport)))
                            : options;
                    })
               .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var gatewayTransportDependencyContainer = gatewayHost.GetTransportDependencyContainer();

            Assert.NotEqual(transportDependencyContainer.Resolve<EndpointIdentity>(), gatewayTransportDependencyContainer.Resolve<EndpointIdentity>());
            Assert.NotEqual(transportDependencyContainer.Resolve<EndpointIdentity>().LogicalName, gatewayTransportDependencyContainer.Resolve<EndpointIdentity>().LogicalName);

            using (host)
            using (gatewayHost)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = Task.WhenAll(
                    host.WaitUntilTransportIsNotRunning(Output.WriteLine),
                    gatewayHost.WaitUntilTransportIsNotRunning(Output.WriteLine));

                await Task.WhenAll(
                        host.StartAsync(cts.Token),
                        gatewayHost.StartAsync(cts.Token))
                   .ConfigureAwait(false);

                var hostShutdown = Task.WhenAll(
                    host.WaitForShutdownAsync(cts.Token),
                    gatewayHost.WaitForShutdownAsync(cts.Token));

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                await Task.WhenAll(
                        Run(transportDependencyContainer, settingsScope, hostShutdown, cts.Token),
                        Run(gatewayTransportDependencyContainer, settingsScope, hostShutdown, cts.Token))
                   .ConfigureAwait(false);

                var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();
                Assert.Empty(collector.ErrorMessages);

                var gatewayCollector = gatewayTransportDependencyContainer.Resolve<TestMessagesCollector>();
                Assert.Empty(gatewayCollector.ErrorMessages);

                var messages = collector.Messages
                   .Concat(gatewayCollector.Messages)
                   .Distinct()
                   .ToArray();

                Assert.Equal(4, messages.Length);

                var firstQuery = messages.Where(message => message.ReflectedType == typeof(Query) && message.ReadRequiredHeader<SentFrom>().Value.Equals(transportEndpointIdentity)).ToArray();
                Assert.Single(firstQuery);
                Assert.Single(messages.Where(message => message.ReflectedType == typeof(Reply) && message.ReadRequiredHeader<InitiatorMessageId>().Value.Equals(firstQuery.Single().ReadRequiredHeader<Id>().Value)));

                var secondQuery = messages.Where(message => message.ReflectedType == typeof(Query) && message.ReadRequiredHeader<SentFrom>().Value.Equals(gatewayTransportEndpointIdentity)).ToArray();
                Assert.Single(secondQuery);
                Assert.Single(messages.Where(message => message.ReflectedType == typeof(Reply) && message.ReadRequiredHeader<InitiatorMessageId>().Value.Equals(secondQuery.Single().ReadRequiredHeader<Id>().Value)));

                await Task.WhenAll(
                        host.StopAsync(cts.Token),
                        gatewayHost.StopAsync(cts.Token))
                   .ConfigureAwait(false);

                await hostShutdown.ConfigureAwait(false);
            }

            static async Task Run(
                IDependencyContainer transportDependencyContainer,
                string settingsScope,
                Task hostShutdown,
                CancellationToken token)
            {
                var query = new Query(42);

                Reply reply;

                await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var rabbitMqSettings = await transportDependencyContainer
                       .Resolve<ISettingsProvider<RabbitMqSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(settingsScope, rabbitMqSettings.VirtualHost);

                    var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                    var awaiter = Task.WhenAny(
                        hostShutdown,
                        integrationContext.RpcRequest<Query, Reply>(query, token));

                    var result = await awaiter.ConfigureAwait(false);

                    if (hostShutdown == result)
                    {
                        throw new InvalidOperationException("Host was unexpectedly stopped");
                    }

                    reply = await ((Task<Reply>)result).ConfigureAwait(false);
                }

                Assert.Equal(query.Id, reply.Id);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task ContravariantMessageHandlerTest(
            Func<EndpointIdentity, string, ILogger, IHostBuilder, Func<DependencyContainerOptions, DependencyContainerOptions>, IHostBuilder> useTransport,
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
                    new EndpointIdentity(TransportEndpointIdentity.LogicalName, Guid.NewGuid()),
                    settingsScope,
                    logger,
                    Fixture.CreateHostBuilder(Output),
                    options => options)
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
                var rabbitMqSettings = await transportDependencyContainer
                   .Resolve<ISettingsProvider<RabbitMqSettings>>()
                   .Get(cts.Token)
                   .ConfigureAwait(false);

                Assert.Equal(settingsScope, rabbitMqSettings.VirtualHost);

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

                var result = await awaiter.ConfigureAwait(false);

                if (hostShutdown == result)
                {
                    throw new InvalidOperationException("Host was unexpectedly stopped");
                }

                await result.ConfigureAwait(false);

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

                await hostShutdown.ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task ThrowingMessageHandlerTest(
            Func<EndpointIdentity, string, ILogger, IHostBuilder, Func<DependencyContainerOptions, DependencyContainerOptions>, IHostBuilder> useTransport,
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

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(settingsScope),
                Fixture.DelegateOverride(container =>
                {
                    container.Override<IRetryPolicy, TestRetryPolicy>(EnLifestyle.Singleton);
                })
            };

            var host = useTransport(
                    new EndpointIdentity(TransportEndpointIdentity.LogicalName, Guid.NewGuid()),
                    settingsScope,
                    logger,
                    Fixture.CreateHostBuilder(Output),
                    options => options)
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
                var rabbitMqSettings = await transportDependencyContainer
                   .Resolve<ISettingsProvider<RabbitMqSettings>>()
                   .Get(cts.Token)
                   .ConfigureAwait(false);

                Assert.Equal(settingsScope, rabbitMqSettings.VirtualHost);

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

                var result = await awaiter.ConfigureAwait(false);

                if (hostShutdown == result)
                {
                    throw new InvalidOperationException("Host was unexpectedly stopped");
                }

                await result.ConfigureAwait(false);

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
                        Output.WriteLine($"{0.95 * expected} ({0.95} * {expected}) <= {actual}");
                        return 0.95 * expected <= actual;
                    }));

                await host.StopAsync(cts.Token).ConfigureAwait(false);

                await hostShutdown.ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task EventSubscriptionBetweenEndpointsTest(
            Func<EndpointIdentity, string, ILogger, IHostBuilder, Func<DependencyContainerOptions, DependencyContainerOptions>, IHostBuilder> useTransport,
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
                    new EndpointIdentity(TransportEndpointIdentity.LogicalName, Guid.NewGuid()),
                    settingsScope,
                    logger,
                    Fixture.CreateHostBuilder(Output),
                    options => options)
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
                var rabbitMqSettings = await transportDependencyContainer
                   .Resolve<ISettingsProvider<RabbitMqSettings>>()
                   .Get(cts.Token)
                   .ConfigureAwait(false);

                Assert.Equal(settingsScope, rabbitMqSettings.VirtualHost);

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

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task RunTest(
            Func<EndpointIdentity, string, ILogger, IHostBuilder, Func<DependencyContainerOptions, DependencyContainerOptions>, IHostBuilder> useTransport,
            TimeSpan timeout)
        {
            var logger = Fixture.CreateLogger(Output);

            var settingsScope = nameof(RunTest);

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(settingsScope)
            };

            var host = useTransport(
                    new EndpointIdentity(TransportEndpointIdentity.LogicalName, Guid.NewGuid()),
                    settingsScope,
                    logger,
                    Fixture.CreateHostBuilder(Output),
                    options => options)
               .UseEndpoint(new EndpointIdentity(settingsScope, 0),
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
                var rabbitMqSettings = await transportDependencyContainer
                   .Resolve<ISettingsProvider<RabbitMqSettings>>()
                   .Get(cts.Token)
                   .ConfigureAwait(false);

                Assert.Equal(settingsScope, rabbitMqSettings.VirtualHost);

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
            Func<EndpointIdentity, string, ILogger, IHostBuilder, Func<DependencyContainerOptions, DependencyContainerOptions>, IHostBuilder> useTransport,
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
                    new EndpointIdentity(TransportEndpointIdentity.LogicalName, Guid.NewGuid()),
                    settingsScope,
                    logger,
                    Fixture.CreateHostBuilder(Output),
                    options => options)
               .UseEndpoint(new EndpointIdentity(settingsScope, 0),
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
                var rabbitMqSettings = await transportDependencyContainer
                   .Resolve<ISettingsProvider<RabbitMqSettings>>()
                   .Get(cts.Token)
                   .ConfigureAwait(false);

                Assert.Equal(settingsScope, rabbitMqSettings.VirtualHost);

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