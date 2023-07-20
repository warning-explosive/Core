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
    using GenericEndpoint.Api.Abstractions;
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
    using Xunit.Sdk;

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
        /// Test cases for RunHostTest
        /// </summary>
        /// <returns>Test cases</returns>
        public static IEnumerable<object[]> RunHostTestData()
        {
            Func<IXunitTestCase, DirectoryInfo> settingsDirectoryProducer =
                testCase =>
                {
                    var projectFileDirectory = SolutionExtensions.ProjectFile().Directory
                                               ?? throw new InvalidOperationException("Project directory wasn't found");

                    return projectFileDirectory
                        .StepInto("Settings")
                        .StepInto(testCase.Method.Name);
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
                new object[] { inMemoryIntegrationTransportIdentity, useInMemoryIntegrationTransport },
                new object[] { rabbitMqIntegrationTransportIdentity, useRabbitMqIntegrationTransport }
            };

            return integrationTransportProviders
               .Select(transport =>
               {
                   var (transportIdentity, useTransport, _) = transport;

                   return new object[]
                   {
                       settingsDirectoryProducer,
                       transportIdentity,
                       useTransport
                   };
               });
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task RequestReplyTest(
            Func<IXunitTestCase, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport)
        {
            var settingsDirectory = settingsDirectoryProducer(TestCase);

            var messageTypes = new[]
            {
                typeof(MakeRequestCommand),
                typeof(Request),
                typeof(Reply)
            };

            var messageHandlerTypes = new[]
            {
                typeof(MakeRequestCommandHandler),
                typeof(AlwaysReplyMessageHandler),
                typeof(ReplyEmptyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport)
                .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
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

                    var rabbitMqSettings = transportDependencyContainer
                        .Resolve<ISettingsProvider<RabbitMqSettings>>()
                        .Get();

                    if (transportDependencyContainer.Resolve<IIntegrationTransport>() is RabbitMqIntegrationTransport)
                    {
                        Assert.Equal(settingsDirectory.Name, rabbitMqSettings.VirtualHost);
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                        var command = new MakeRequestCommand(42);

                        var awaiter = Task.WhenAll(
                            collector.WaitUntilMessageIsNotReceived<Reply>(message => message.Id == command.Id),
                            collector.WaitUntilErrorMessageIsNotReceived<Endpoint1HandlerInvoked>(message => message.HandlerType == typeof(ReplyEmptyMessageHandler)));

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

                        var errorMessages = collector.ErrorMessages.ToArray();
                        Assert.Single(errorMessages);
                        Assert.Single(errorMessages.Where(info => info.message.ReflectedType == typeof(Endpoint1HandlerInvoked) && ((Endpoint1HandlerInvoked)info.message.Payload).HandlerType == typeof(ReplyEmptyMessageHandler)));

                        var messages = collector.Messages.ToArray();
                        Assert.Equal(3, messages.Length);
                        Assert.Single(messages.Where(message => message.ReflectedType == typeof(MakeRequestCommand)));
                        Assert.Single(messages.Where(message => message.ReflectedType == typeof(Request)));
                        Assert.Single(messages.Where(message => message.ReflectedType == typeof(Reply)));
                    }
                };
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task RpcRequestTest(
            Func<IXunitTestCase, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport)
        {
            var settingsDirectory = settingsDirectoryProducer(TestCase);

            var messageTypes = new[]
            {
                typeof(Request),
                typeof(Reply)
            };

            var messageHandlerTypes = new[]
            {
                typeof(AlwaysReplyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport)
                .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
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

                    var rabbitMqSettings = transportDependencyContainer
                        .Resolve<ISettingsProvider<RabbitMqSettings>>()
                        .Get();

                    if (transportDependencyContainer.Resolve<IIntegrationTransport>() is RabbitMqIntegrationTransport)
                    {
                        Assert.Equal(settingsDirectory.Name, rabbitMqSettings.VirtualHost);
                    }

                    var request = new Request(42);

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();
                        var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                        var reply = await integrationContext
                            .RpcRequest<Request, Reply>(request, token)
                            .ConfigureAwait(false);

                        Assert.Equal(request.Id, reply.Id);
                        Assert.Empty(collector.ErrorMessages);

                        var messages = collector.Messages.ToArray();
                        Assert.Equal(2, messages.Length);

                        var integrationRequest = messages.Where(message => message.ReflectedType == typeof(Request) && message.ReadRequiredHeader<SentFrom>().Value.LogicalName.Equals(Identity.LogicalName, StringComparison.OrdinalIgnoreCase)).ToArray();
                        Assert.Single(integrationRequest);
                        Assert.Single(messages.Where(message => message.ReflectedType == typeof(Reply) && message.ReadRequiredHeader<InitiatorMessageId>().Value.Equals(integrationRequest.Single().ReadRequiredHeader<Id>().Value)));
                    }
                };
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task ContravariantMessageHandlerTest(
            Func<IXunitTestCase, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport)
        {
            var settingsDirectory = settingsDirectoryProducer(TestCase);

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

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport)
                .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
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

                    var rabbitMqSettings = transportDependencyContainer
                        .Resolve<ISettingsProvider<RabbitMqSettings>>()
                        .Get();

                    if (transportDependencyContainer.Resolve<IIntegrationTransport>() is RabbitMqIntegrationTransport)
                    {
                        Assert.Equal(settingsDirectory.Name, rabbitMqSettings.VirtualHost);
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                        var awaiter = Task.WhenAll(
                            collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(message => message.HandlerType == typeof(BaseEventEmptyMessageHandler)),
                            collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(message => message.HandlerType == typeof(InheritedEventEmptyMessageHandler)));

                        var integrationMessage = endpointDependencyContainer
                            .Resolve<IIntegrationMessageFactory>()
                            .CreateGeneralMessage(
                                new PublishInheritedEventCommand(42),
                                typeof(PublishInheritedEventCommand),
                                Array.Empty<IIntegrationMessageHeader>(),
                                null);

                        await endpointDependencyContainer
                            .Resolve<IIntegrationTransport>()
                            .Enqueue(integrationMessage, token)
                            .ConfigureAwait(false);

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
                    }
                };
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task ThrowingMessageHandlerTest(
            Func<IXunitTestCase, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport)
        {
            var settingsDirectory = settingsDirectoryProducer(TestCase);

            var messageTypes = new[]
            {
                typeof(Command)
            };

            var messageHandlerTypes = new[]
            {
                typeof(CommandThrowingMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport)
                .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
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

                    var rabbitMqSettings = transportDependencyContainer
                        .Resolve<ISettingsProvider<RabbitMqSettings>>()
                        .Get();

                    if (transportDependencyContainer.Resolve<IIntegrationTransport>() is RabbitMqIntegrationTransport)
                    {
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

                        await endpointDependencyContainer
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
        [MemberData(nameof(RunHostTestData))]
        internal async Task EventSubscriptionBetweenEndpointsTest(
            Func<IXunitTestCase, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport)
        {
            var settingsDirectory = settingsDirectoryProducer(TestCase);

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

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport)
                .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(endpoint1AdditionalOurTypes))
                        .BuildOptions())
                .UseEndpoint(TestIdentity.Endpoint20,
                    (_, builder) => builder
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

                    var rabbitMqSettings = transportDependencyContainer
                        .Resolve<ISettingsProvider<RabbitMqSettings>>()
                        .Get();

                    if (transportDependencyContainer.Resolve<IIntegrationTransport>() is RabbitMqIntegrationTransport)
                    {
                        Assert.Equal(settingsDirectory.Name, rabbitMqSettings.VirtualHost);
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                        var awaiter = Task.WhenAll(
                            collector.WaitUntilMessageIsNotReceived<Event>(),
                            collector.WaitUntilMessageIsNotReceived<Endpoint1HandlerInvoked>(message => message.EndpointIdentity.Equals(TestIdentity.Endpoint10)),
                            collector.WaitUntilMessageIsNotReceived<Endpoint2HandlerInvoked>(message => message.EndpointIdentity.Equals(TestIdentity.Endpoint20)));

                        var integrationMessage = endpointDependencyContainer
                            .Resolve<IIntegrationMessageFactory>()
                            .CreateGeneralMessage(
                                new PublishEventCommand(42),
                                typeof(PublishEventCommand),
                                Array.Empty<IIntegrationMessageHeader>(),
                                null);

                        await endpointDependencyContainer
                            .Resolve<IIntegrationTransport>()
                            .Enqueue(integrationMessage, token)
                            .ConfigureAwait(false);

                        await awaiter.ConfigureAwait(false);
                    }
                };
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostTestData))]
        internal async Task StartStopTest(
            Func<IXunitTestCase, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport)
        {
            var settingsDirectory = settingsDirectoryProducer(TestCase);

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport)
                .UseEndpoint(TestIdentity.Endpoint10, (_, builder) => builder.BuildOptions())
                .BuildHost(settingsDirectory);

            await host
                .RunTestHost(Output, TestCase, StartStopTestInternal(settingsDirectory, transportIdentity))
                .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> StartStopTestInternal(
                DirectoryInfo settingsDirectory,
                TransportIdentity transportIdentity)
            {
                return async (_, host, _) =>
                {
                    var transportDependencyContainer = host.GetIntegrationTransportDependencyContainer(transportIdentity);

                    var rabbitMqSettings = transportDependencyContainer
                        .Resolve<ISettingsProvider<RabbitMqSettings>>()
                        .Get();

                    if (transportDependencyContainer.Resolve<IIntegrationTransport>() is RabbitMqIntegrationTransport)
                    {
                        Assert.Equal(settingsDirectory.Name, rabbitMqSettings.VirtualHost);
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                        Assert.Empty(collector.ErrorMessages);
                        Assert.Empty(collector.Messages);
                    }
                };
            }
        }
    }
}