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
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CrossCuttingConcerns.Settings;
    using Extensions;
    using GenericEndpoint.Host;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;
    using GenericEndpoint.Pipeline;
    using GenericHost;
    using IntegrationTransport.Api;
    using IntegrationTransport.Api.Abstractions;
    using IntegrationTransport.Host;
    using IntegrationTransport.RabbitMQ;
    using IntegrationTransport.RabbitMQ.Settings;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.Hosting;
    using Mocks;
    using Registrations;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
    using Xunit;
    using Xunit.Abstractions;
    using EventHandler = MessageHandlers.EventHandler;

    /// <summary>
    /// Endpoint communication tests
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
    public class EndpointCommunicationTests : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public EndpointCommunicationTests(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// Test cases for endpoint communication tests
        /// </summary>
        /// <returns>Test cases</returns>
        public static IEnumerable<object[]> EndpointCommunicationTestData()
        {
            Func<string, DirectoryInfo> settingsDirectoryProducer =
                testDirectory =>
                {
                    var projectFileDirectory = SolutionExtensions.ProjectFile().Directory
                                               ?? throw new InvalidOperationException("Project directory wasn't found");

                    return projectFileDirectory
                        .StepInto("Settings")
                        .StepInto(testDirectory);
                };

            var inMemoryIntegrationTransportIdentity = IntegrationTransport.InMemory.Identity.TransportIdentity();

            var useInMemoryIntegrationTransport = new Func<IHostBuilder, TransportIdentity, IHostBuilder>(
                static (hostBuilder, transportIdentity) => hostBuilder.UseInMemoryIntegrationTransport(
                    transportIdentity,
                    options => options
                        .WithManualRegistrations(new MessagesCollectorManualRegistration())));

            var rabbitMqIntegrationTransportIdentity = IntegrationTransport.RabbitMQ.Identity.TransportIdentity();

            var useRabbitMqIntegrationTransport = new Func<IHostBuilder, TransportIdentity, IHostBuilder>(
                static (hostBuilder, transportIdentity) => hostBuilder.UseRabbitMqIntegrationTransport(
                    transportIdentity,
                    builder => builder
                        .WithManualRegistrations(new PurgeRabbitMqQueuesManualRegistration())
                        .WithManualRegistrations(new MessagesCollectorManualRegistration())));

            var integrationTransportProviders = new[]
            {
                (inMemoryIntegrationTransportIdentity, useInMemoryIntegrationTransport),
                (rabbitMqIntegrationTransportIdentity, useRabbitMqIntegrationTransport)
            };

            return integrationTransportProviders
               .Select(transport =>
               {
                   var (transportIdentity, useTransport) = transport;

                   return new object[]
                   {
                       settingsDirectoryProducer,
                       transportIdentity,
                       useTransport
                   };
               });
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(EndpointCommunicationTestData))]
        internal async Task Endpoint_supports_request_reply_communication_pattern(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport)
        {
            var settingsDirectory = settingsDirectoryProducer("EndpointSupportsRequestReplyCommunicationPattern");

            var messageTypes = new[]
            {
                typeof(MakeRequestCommand),
                typeof(Request),
                typeof(Reply),
                typeof(HandlerInvoked)
            };

            var messageHandlerTypes = new[]
            {
                typeof(MakeRequestCommandHandler),
                typeof(AlwaysReplyRequestHandler),
                typeof(ReplyHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport)
                .UseEndpoint(TestIdentity.Endpoint10,
                    builder => builder
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(additionalOurTypes))
                        .BuildOptions())
                .BuildHost(settingsDirectory);

            await host
                .RunTestHost(Output, TestCase, RequestReplyTestInternal(settingsDirectory, transportIdentity))
                .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> RequestReplyTestInternal(
                DirectoryInfo settingsDirectory,
                TransportIdentity transportIdentity)
            {
                return async (_, host, token) =>
                {
                    var transportDependencyContainer = host.GetIntegrationTransportDependencyContainer(transportIdentity);
                    var endpointDependencyContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                    if (transportDependencyContainer.Resolve<IIntegrationTransport>() is RabbitMqIntegrationTransport)
                    {
                        var rabbitMqSettings = transportDependencyContainer
                            .Resolve<ISettingsProvider<RabbitMqSettings>>()
                            .Get();

                        Assert.Equal(settingsDirectory.Name, rabbitMqSettings.VirtualHost);
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                        var command = new MakeRequestCommand(42);

                        var awaiter = Task.WhenAll(
                            collector.WaitUntilMessageIsNotReceived<MakeRequestCommand>(message => message.Id == command.Id),
                            collector.WaitUntilMessageIsNotReceived<Request>(message => message.Id == command.Id),
                            collector.WaitUntilMessageIsNotReceived<Reply>(message => message.Id == command.Id),
                            collector.WaitUntilErrorMessageIsNotReceived<HandlerInvoked>(message => message.HandlerType == typeof(ReplyHandler) && message.EndpointIdentity == TestIdentity.Endpoint10));

                        var integrationMessage = endpointDependencyContainer
                            .Resolve<IIntegrationMessageFactory>()
                            .CreateGeneralMessage(
                                command,
                                typeof(MakeRequestCommand),
                                Array.Empty<IIntegrationMessageHeader>(),
                                null);

                        await transportDependencyContainer
                            .Resolve<IIntegrationTransport>()
                            .Enqueue(integrationMessage, token)
                            .ConfigureAwait(false);

                        await awaiter.ConfigureAwait(false);
                    }
                };
            }
        }

        // TODO: #205 - implement rpc-transport
        [SuppressMessage("Analysis", "xUnit1004", Justification = "#205")]
        [Theory(Skip = "#205", Timeout = 60_000)]
        [MemberData(nameof(EndpointCommunicationTestData))]
        internal async Task Endpoint_supports_remote_procedure_calls(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport)
        {
            var settingsDirectory = settingsDirectoryProducer("EndpointSupportsRemoteProcedureCalls");

            var messageTypes = new[]
            {
                typeof(MakeRpcRequestCommand),
                typeof(Request),
                typeof(Reply),
                typeof(HandlerInvoked)
            };

            var messageHandlerTypes = new[]
            {
                typeof(MakeRpcRequestCommandHandler),
                typeof(AlwaysReplyRequestHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport)
                .UseEndpoint(TestIdentity.Endpoint10,
                    builder => builder
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(additionalOurTypes))
                        .BuildOptions())
                .BuildHost(settingsDirectory);

            await host
                .RunTestHost(Output, TestCase, RpcRequestTestInternal(settingsDirectory, transportIdentity))
                .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> RpcRequestTestInternal(
                DirectoryInfo settingsDirectory,
                TransportIdentity transportIdentity)
            {
                return async (_, host, token) =>
                {
                    var transportDependencyContainer = host.GetIntegrationTransportDependencyContainer(transportIdentity);
                    var endpointDependencyContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                    if (transportDependencyContainer.Resolve<IIntegrationTransport>() is RabbitMqIntegrationTransport)
                    {
                        var rabbitMqSettings = transportDependencyContainer
                            .Resolve<ISettingsProvider<RabbitMqSettings>>()
                            .Get();

                        Assert.Equal(settingsDirectory.Name, rabbitMqSettings.VirtualHost);
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                        var command = new MakeRpcRequestCommand(42);

                        var integrationMessage = endpointDependencyContainer
                            .Resolve<IIntegrationMessageFactory>()
                            .CreateGeneralMessage(
                                command,
                                typeof(MakeRpcRequestCommand),
                                Array.Empty<IIntegrationMessageHeader>(),
                                null);

                        var awaiter = Task.WhenAll(
                            collector.WaitUntilMessageIsNotReceived<MakeRpcRequestCommand>(message => message.Id == command.Id),
                            collector.WaitUntilMessageIsNotReceived(message => message.ReflectedType == typeof(Request) && ((Request)message.Payload).Id == command.Id && message.ReadRequiredHeader<SentFrom>().Value.LogicalName.Equals(TestIdentity.Endpoint10.LogicalName, StringComparison.OrdinalIgnoreCase)),
                            collector.WaitUntilMessageIsNotReceived(message => message.ReflectedType == typeof(Reply) && ((Reply)message.Payload).Id == command.Id && message.ReadRequiredHeader<InitiatorMessageId>().Value.Equals(integrationMessage.ReadRequiredHeader<Id>().Value)),
                            collector.WaitUntilErrorMessageIsNotReceived<HandlerInvoked>(message => message.HandlerType == typeof(MakeRpcRequestCommandHandler) && message.EndpointIdentity == TestIdentity.Endpoint10));

                        await transportDependencyContainer
                            .Resolve<IIntegrationTransport>()
                            .Enqueue(integrationMessage, token)
                            .ConfigureAwait(false);

                        await awaiter.ConfigureAwait(false);
                    }
                };
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(EndpointCommunicationTestData))]
        internal async Task Endpoint_supports_contravariant_messaging(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport)
        {
            var settingsDirectory = settingsDirectoryProducer("EndpointSupportsContravariantMessaging");

            var messageTypes = new[]
            {
                typeof(PublishInheritedEventCommand),
                typeof(BaseEvent),
                typeof(InheritedEvent),
                typeof(HandlerInvoked)
            };

            var messageHandlerTypes = new[]
            {
                typeof(PublishInheritedEventCommandHandler),
                typeof(BaseEventHandler),
                typeof(InheritedEventHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport)
                .UseEndpoint(TestIdentity.Endpoint10,
                    builder => builder
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(additionalOurTypes))
                        .BuildOptions())
                .BuildHost(settingsDirectory);

            await host
                .RunTestHost(Output, TestCase, ContravariantMessageHandlerTestInternal(settingsDirectory, transportIdentity))
                .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> ContravariantMessageHandlerTestInternal(
                DirectoryInfo settingsDirectory,
                TransportIdentity transportIdentity)
            {
                return async (_, host, token) =>
                {
                    var transportDependencyContainer = host.GetIntegrationTransportDependencyContainer(transportIdentity);
                    var endpointDependencyContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                    if (transportDependencyContainer.Resolve<IIntegrationTransport>() is RabbitMqIntegrationTransport)
                    {
                        var rabbitMqSettings = transportDependencyContainer
                            .Resolve<ISettingsProvider<RabbitMqSettings>>()
                            .Get();

                        Assert.Equal(settingsDirectory.Name, rabbitMqSettings.VirtualHost);
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                        var awaiter = Task.WhenAll(
                            collector.WaitUntilMessageIsNotReceived(message => message.ReflectedType == typeof(PublishInheritedEventCommand)),
                            collector.WaitUntilMessageIsNotReceived(message => message.Payload.GetType() == typeof(InheritedEvent) && message.ReflectedType == typeof(BaseEvent)),
                            collector.WaitUntilMessageIsNotReceived(message => message.Payload.GetType() == typeof(InheritedEvent) && message.ReflectedType == typeof(InheritedEvent)),
                            collector.WaitUntilErrorMessageIsNotReceived<HandlerInvoked>(message => message.HandlerType == typeof(BaseEventHandler) && message.EndpointIdentity == TestIdentity.Endpoint10),
                            collector.WaitUntilErrorMessageIsNotReceived<HandlerInvoked>(message => message.HandlerType == typeof(InheritedEventHandler) && message.EndpointIdentity == TestIdentity.Endpoint10));

                        var integrationMessage = endpointDependencyContainer
                            .Resolve<IIntegrationMessageFactory>()
                            .CreateGeneralMessage(
                                new PublishInheritedEventCommand(42),
                                typeof(PublishInheritedEventCommand),
                                Array.Empty<IIntegrationMessageHeader>(),
                                null);

                        await transportDependencyContainer
                            .Resolve<IIntegrationTransport>()
                            .Enqueue(integrationMessage, token)
                            .ConfigureAwait(false);

                        await awaiter.ConfigureAwait(false);
                    }
                };
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(EndpointCommunicationTestData))]
        internal async Task Endpoint_supports_custom_retry_policies(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport)
        {
            var settingsDirectory = settingsDirectoryProducer("EndpointSupportsCustomRetryPolicies");

            var messageTypes = new[]
            {
                typeof(Command)
            };

            var messageHandlerTypes = new[]
            {
                typeof(ThrowingCommandHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport)
                .UseEndpoint(TestIdentity.Endpoint10,
                    builder => builder
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(additionalOurTypes)
                            .WithOverrides(Fixture.DelegateOverride(container => container
                                .Override<IRetryPolicy, TestRetryPolicy>(EnLifestyle.Singleton))))
                        .BuildOptions())
                .BuildHost(settingsDirectory);

            await host
                .RunTestHost(Output, TestCase, ThrowingMessageHandlerTestInternal(settingsDirectory, transportIdentity))
                .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> ThrowingMessageHandlerTestInternal(
                DirectoryInfo settingsDirectory,
                TransportIdentity transportIdentity)
            {
                return async (output, host, token) =>
                {
                    var transportDependencyContainer = host.GetIntegrationTransportDependencyContainer(transportIdentity);
                    var endpointDependencyContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                    if (transportDependencyContainer.Resolve<IIntegrationTransport>() is RabbitMqIntegrationTransport)
                    {
                        var rabbitMqSettings = transportDependencyContainer
                            .Resolve<ISettingsProvider<RabbitMqSettings>>()
                            .Get();

                        Assert.Equal(settingsDirectory.Name, rabbitMqSettings.VirtualHost);
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                        var awaiter = collector.WaitUntilErrorMessageIsNotReceived<Command>();

                        var integrationMessage = endpointDependencyContainer
                            .Resolve<IIntegrationMessageFactory>()
                            .CreateGeneralMessage(
                                new Command(42),
                                typeof(Command),
                                Array.Empty<IIntegrationMessageHeader>(),
                                null);

                        await transportDependencyContainer
                            .Resolve<IIntegrationTransport>()
                            .Enqueue(integrationMessage, token)
                            .ConfigureAwait(false);

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
                    }
                };
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(EndpointCommunicationTestData))]
        internal async Task Endpoint_supports_publish_subscribe_communication_pattern(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport)
        {
            var settingsDirectory = settingsDirectoryProducer("EndpointSupportsPublishSubscribeCommunicationPattern");

            var endpoint1MessageTypes = new[]
            {
                typeof(Event),
                typeof(HandlerInvoked)
            };

            var endpoint1MessageHandlerTypes = new[]
            {
                typeof(EventHandler)
            };

            var endpoint2MessageTypes = new[]
            {
                typeof(Event),
                typeof(PublishEventCommand),
                typeof(HandlerInvoked)
            };

            var endpoint2MessageHandlerTypes = new[]
            {
                typeof(PublishEventCommandHandler),
                typeof(EventHandler)
            };

            var endpoint1AdditionalOurTypes = endpoint1MessageTypes.Concat(endpoint1MessageHandlerTypes).ToArray();
            var endpoint2AdditionalOurTypes = endpoint2MessageTypes.Concat(endpoint2MessageHandlerTypes).ToArray();

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport)
                .UseEndpoint(TestIdentity.Endpoint10,
                    builder => builder
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(endpoint1AdditionalOurTypes))
                        .BuildOptions())
                .UseEndpoint(TestIdentity.Endpoint20,
                    builder => builder
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(endpoint2AdditionalOurTypes))
                        .BuildOptions())
                .BuildHost(settingsDirectory);

            await host
                .RunTestHost(Output, TestCase, EventSubscriptionBetweenEndpointsTestInternal(settingsDirectory, transportIdentity))
                .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> EventSubscriptionBetweenEndpointsTestInternal(
                DirectoryInfo settingsDirectory,
                TransportIdentity transportIdentity)
            {
                return async (_, host, token) =>
                {
                    var transportDependencyContainer = host.GetIntegrationTransportDependencyContainer(transportIdentity);
                    var endpointDependencyContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                    if (transportDependencyContainer.Resolve<IIntegrationTransport>() is RabbitMqIntegrationTransport)
                    {
                        var rabbitMqSettings = transportDependencyContainer
                            .Resolve<ISettingsProvider<RabbitMqSettings>>()
                            .Get();

                        Assert.Equal(settingsDirectory.Name, rabbitMqSettings.VirtualHost);
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                        var command = new PublishEventCommand(42);

                        var awaiter = Task.WhenAll(
                            collector.WaitUntilMessageIsNotReceived<PublishEventCommand>(message => message.Id == command.Id),
                            collector.WaitUntilMessageIsNotReceived<Event>(message => message.Id == command.Id),
                            collector.WaitUntilErrorMessageIsNotReceived<HandlerInvoked>(message => message.HandlerType == typeof(EventHandler) && message.EndpointIdentity == TestIdentity.Endpoint10),
                            collector.WaitUntilErrorMessageIsNotReceived<HandlerInvoked>(message => message.HandlerType == typeof(EventHandler) && message.EndpointIdentity == TestIdentity.Endpoint20));

                        var integrationMessage = endpointDependencyContainer
                            .Resolve<IIntegrationMessageFactory>()
                            .CreateGeneralMessage(
                                command,
                                typeof(PublishEventCommand),
                                Array.Empty<IIntegrationMessageHeader>(),
                                null);

                        await transportDependencyContainer
                            .Resolve<IIntegrationTransport>()
                            .Enqueue(integrationMessage, token)
                            .ConfigureAwait(false);

                        await awaiter.ConfigureAwait(false);
                    }
                };
            }
        }
    }
}