namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using AutoWiring.Api.Services;
    using Basics;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using CrossCuttingConcerns;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;
    using GenericEndpoint.Executable;
    using GenericEndpoint.Executable.Abstractions;
    using GenericEndpoint.TestExtensions;
    using GenericHost;
    using GenericHost.Abstractions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Mocks;
    using Registrations;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// GenericHost assembly tests
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "For test reasons")]
    public class GenericHostTest : TestBase
    {
        private const string Endpoint1 = nameof(Endpoint1);
        private const string Endpoint2 = nameof(Endpoint2);

        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public GenericHostTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary> TransportTestData member </summary>
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> TransportTestData()
        {
            yield return new object[] { Core.GenericHost.InMemoryIntegrationTransport.Transport.InMemoryIntegrationTransport() };
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task SameTransportTest(IIntegrationTransport transport)
        {
            var assembly = typeof(IExecutableEndpoint).Assembly; // GenericEndpoint.Executable

            var options10 = new EndpointOptions(new EndpointIdentity(Endpoint1, 0), new DependencyContainerOptions(), assembly);
            var options20 = new EndpointOptions(new EndpointIdentity(Endpoint2, 0), new DependencyContainerOptions(), assembly);

            var transportIsSame = false;

            using (var host = Host.CreateDefaultBuilder().ConfigureHost(transport, options10, options20).Build())
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                transportIsSame = host.Services
                    .GetRequiredService<IHostedService>()
                    .GetPropertyValue<IReadOnlyDictionary<string, IReadOnlyCollection<IGenericEndpoint>>>("Endpoints")
                    .SelectMany(logicalGroup => logicalGroup.Value)
                    .Select(endpoint => endpoint.GetPropertyValue<IDependencyContainer>("DependencyContainer"))
                    .Select(container => container.Resolve<IIntegrationTransport>())
                    .All(endpointTransport => ReferenceEquals(transport, endpointTransport));

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }

            Assert.True(transportIsSame);
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task IntegrationContextCheckAppliedDecoratorTest(IIntegrationTransport transport)
        {
            var transportDependencyContainer = transport.GetPropertyValue<IDependencyContainer>("DependencyContainer");
            var integrationMessage = new IntegrationMessage(new TestCommand(0), typeof(TestCommand), new StringFormatterMock());

            // 1 - IUbiquitousIntegrationContext
            Assert.Throws<InvalidOperationException>(() => transportDependencyContainer.Resolve<IAdvancedIntegrationContext>());
            Assert.Throws<SimpleInjector.ActivationException>(() => transportDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage));
            Assert.Throws<SimpleInjector.ActivationException>(() => transportDependencyContainer.Resolve<IIntegrationContext>());
            var ubiquitousIntegrationContext = transportDependencyContainer.Resolve<IUbiquitousIntegrationContext>();

            Assert.NotNull(ubiquitousIntegrationContext);

            var inMemoryTransportAssemblyName = AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericHost), nameof(Core.GenericHost.InMemoryIntegrationTransport));
            var inMemoryUbiquitousIntegrationContextTypeFullName = AssembliesExtensions.BuildName(inMemoryTransportAssemblyName, "Internals", "InMemoryUbiquitousIntegrationContext");
            var integrationContextType = AssembliesExtensions.FindRequiredType(inMemoryTransportAssemblyName, inMemoryUbiquitousIntegrationContextTypeFullName);

            var decorators = ubiquitousIntegrationContext.ExtractDecorators().ToList();
            Assert.Single(decorators);

            var actual = decorators.ShowTypes("#ubiquitous integration context", Output.WriteLine).Single();
            Assert.Equal(integrationContextType, actual);

            // 2 - IAdvancedIntegrationContext
            var endpointIdentity = new EndpointIdentity(nameof(IntegrationContextCheckAppliedDecoratorTest), 0);
            var assembly = typeof(IExecutableEndpoint).Assembly; // GenericEndpoint.Executable
            var endpointOptions = new EndpointOptions(endpointIdentity, new DependencyContainerOptions(), assembly);

            using (var host = Host.CreateDefaultBuilder().ConfigureHost(transport, endpointOptions).Build())
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var hostedService = (IHostedService?)host.Services.GetService(typeof(IHostedService));
                Assert.NotNull(hostedService);
                var endpoints = hostedService!
                    .GetPropertyValue<IReadOnlyDictionary<string, IReadOnlyCollection<IGenericEndpoint>>>("Endpoints")
                    .SelectMany(grp => grp.Value)
                    .ToList();
                Assert.Single(endpoints);
                var endpoint = endpoints.Single();
                var endpointDependencyContainer = endpoint.GetPropertyValue<IDependencyContainer>("DependencyContainer");
                Assert.NotNull(endpointDependencyContainer);

                using (endpointDependencyContainer.OpenScope())
                {
                    Assert.Throws<InvalidOperationException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                    var advancedIntegrationContext = endpointDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage);
                    decorators = advancedIntegrationContext.ExtractDecorators().ShowTypes("#extended integration context", Output.WriteLine).ToList();

                    var genericEndpointAssemblyName = AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericEndpoint));
                    var decoratorTypeName = AssembliesExtensions.BuildName(genericEndpointAssemblyName, "Internals", "AdvancedIntegrationContextHeadersMaintenanceDecorator");
                    var decoratorType = AssembliesExtensions.FindRequiredType(genericEndpointAssemblyName, decoratorTypeName);

                    var contextTypeName = AssembliesExtensions.BuildName(inMemoryTransportAssemblyName, "Internals", "InMemoryIntegrationContext");
                    var contextType = AssembliesExtensions.FindRequiredType(inMemoryTransportAssemblyName, contextTypeName);

                    var expected = new Type[] { decoratorType, contextType };
                    Assert.Equal(expected, decorators);
                }

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal void TestExtensionTest(IIntegrationTransport transport)
        {
            var endpointIdentityRegistration = Fixture.DelegateRegistration(container =>
            {
                var endpointIdentity = new EndpointIdentity(nameof(TestExtensionTest), 0);
                container.RegisterInstance(endpointIdentity);
            });

            var handlersRegistration = Fixture.DelegateRegistration(container =>
            {
                container.Register(typeof(IMessageHandler<>), typeof(TestMessageHandler));
            });

            var options = new DependencyContainerOptions()
                .WithManualRegistration(handlersRegistration)
                .WithManualRegistration(endpointIdentityRegistration)
                .WithManualRegistration(((IAdvancedIntegrationTransport)transport).Injection)
                .WithManualRegistration(new CrossCuttingConcernsManualRegistration())
                .WithManualRegistration(new LoggerTestRegistration());

            var assembly = typeof(IExecutableEndpoint).Assembly; // GenericEndpoint.Executable

            var dependencyContainer = Fixture.BoundedAboveContainer(options, assembly);

            ShouldNotProduceMessages(dependencyContainer.Resolve<IMessageHandler<TestCommand>>().OnMessage(new TestCommand(42))).Invoke();
            ShouldNotProduceMessages(dependencyContainer.Resolve<IMessageHandler<TestEvent>>().OnMessage(new TestEvent(42))).Invoke();
            ShouldNotProduceMessages(dependencyContainer.Resolve<IMessageHandler<TestQuery>>().OnMessage(new TestQuery(42))).Invoke();

            dependencyContainer
                .Resolve<IMessageHandler<TestQuery>>()
                .OnMessage(new TestQuery(43))
                .ShouldNotSend<IIntegrationCommand>()
                .ShouldNotPublish<IIntegrationEvent>()
                .ShouldNotRequest<TestQuery, TestReply>()
                .Replied<TestReply>(reply => reply.Id == 43)
                .Invoke();

            MessageHandlerTestBuilder<T> ShouldNotProduceMessages<T>(MessageHandlerTestBuilder<T> builder)
                where T : IIntegrationMessage
            {
                return builder
                    .ShouldProduceNothing()
                    .ShouldNotSend<IIntegrationCommand>()
                    .ShouldNotPublish<IIntegrationEvent>()
                    .ShouldNotRequest<TestQuery, TestReply>()
                    .ShouldNotReply<IIntegrationMessage>();
            }
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task RunTest(IIntegrationTransport transport)
        {
            using (var runnableHost = BuildHost())
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await runnableHost.RunAsync(cts.Token).ConfigureAwait(false);
            }

            IHost BuildHost()
            {
                var endpointIdentity = new EndpointIdentity(nameof(RunTest), 0);
                var assembly = typeof(IExecutableEndpoint).Assembly; // GenericEndpoint.Executable
                var endpointOptions = new EndpointOptions(endpointIdentity, new DependencyContainerOptions(), assembly);

                return Host
                    .CreateDefaultBuilder()
                    .ConfigureHost(transport, endpointOptions)
                    .Build();
            }
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task StartStopRunTest(IIntegrationTransport transport)
        {
            using (var startStopHost = BuildHost())
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await startStopHost.StartAsync(cts.Token).ConfigureAwait(false);
                await startStopHost.StopAsync(cts.Token).ConfigureAwait(false);
            }

            IHost BuildHost()
            {
                var endpointIdentity = new EndpointIdentity(nameof(StartStopRunTest), 0);
                var assembly = typeof(IExecutableEndpoint).Assembly; // GenericEndpoint.Executable
                var endpointOptions = new EndpointOptions(endpointIdentity, new DependencyContainerOptions(), assembly);

                return Host
                    .CreateDefaultBuilder()
                    .ConfigureHost(transport, endpointOptions)
                    .Build();
            }
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal void InMemoryIntegrationTransportBuildTest(IIntegrationTransport transport)
        {
            var dependencyContainer = transport.GetPropertyValue<IDependencyContainer>("DependencyContainer");
            Assert.NotNull(dependencyContainer);
            Assert.Same(transport, dependencyContainer.Resolve<IIntegrationTransport>());
            Assert.Same(transport.IntegrationContext, dependencyContainer.Resolve<IUbiquitousIntegrationContext>());
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task SimpleHostTest(IIntegrationTransport transport)
        {
            var expectedMessagesCount = 1000;
            var actualMessagesCount = 0;
            var actualRefusedMessagesCount = 0;

            transport.OnMessage += (_, _) =>
            {
                Interlocked.Increment(ref actualMessagesCount);
            };

            transport.OnError += (_, _) =>
            {
                Interlocked.Increment(ref actualRefusedMessagesCount);
            };

            var assembly = typeof(IExecutableEndpoint).Assembly; // GenericEndpoint.Executable

            var additionalTypes = new[]
            {
                typeof(TestCommand),
                typeof(TestEvent),
                typeof(TestQuery),
                typeof(TestReply),
                typeof(TestMessageHandler)
            };

            var endpointIdentity = new EndpointIdentity(nameof(SimpleHostTest), nameof(SimpleHostTest));
            var identityRegistration = Fixture.DelegateRegistration(container => container.RegisterInstance(typeof(EndpointIdentity), endpointIdentity));
            var options = new DependencyContainerOptions()
                .WithManualRegistration(identityRegistration)
                .WithManualRegistration(((IAdvancedIntegrationTransport)transport).Injection)
                .WithManualRegistration(new CrossCuttingConcernsManualRegistration())
                .WithManualRegistration(new LoggerTestRegistration());

            var boundedContainer = Fixture.BoundedAboveContainer(options, assembly);
            var typeProvider = new TypeProviderMock(boundedContainer.Resolve<ITypeProvider>(), additionalTypes);
            var overrides = Fixture.DelegateRegistration(container =>
            {
                container
                    .RegisterInstance(typeof(ITypeProvider), typeProvider)
                    .RegisterInstance(typeProvider.GetType(), typeProvider);
            });

            var handlersRegistration = Fixture.DelegateRegistration(container =>
            {
                container.Register(typeof(IMessageHandler<>), typeof(TestMessageHandler));
            });

            var getContainerOptions = new Func<DependencyContainerOptions>(()
                => new DependencyContainerOptions()
                    .WithManualRegistration(handlersRegistration)
                    .WithOverride(overrides));

            var options10 = new EndpointOptions(new EndpointIdentity(Endpoint1, 0), getContainerOptions(), assembly);
            var options11 = new EndpointOptions(new EndpointIdentity(Endpoint1, 1), getContainerOptions(), assembly);
            var options20 = new EndpointOptions(new EndpointIdentity(Endpoint2, 0), getContainerOptions(), assembly);

            IReadOnlyCollection<FailedMessage> failedMessages;

            using (var host = Host.CreateDefaultBuilder().ConfigureHost(transport, options10, options11, options20).Build())
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await SendInitiationMessages(transport, expectedMessagesCount, cts.Token).ConfigureAwait(false);

                failedMessages = host.Services.GetRequiredService<IHostStatistics>().FailedMessages;

                await Task.Delay(TimeSpan.FromSeconds(3), cts.Token).ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }

            Output.WriteLine($"{nameof(actualMessagesCount)}: {actualMessagesCount}");
            Assert.Equal(expectedMessagesCount, actualMessagesCount);

            Output.WriteLine($"{nameof(actualRefusedMessagesCount)}: {actualRefusedMessagesCount}");
            Assert.Equal(0, actualRefusedMessagesCount);

            Output.WriteLine($"{nameof(IHostStatistics.FailedMessages)}Count: {failedMessages.Count}");
            if (failedMessages.Count > 0) { failedMessages.Each(failedMessage => Output.WriteLine(failedMessage.ToString())); }
            Assert.Equal(0, failedMessages.Count);

            async Task SendInitiationMessages(IIntegrationTransport integrationTransport, int count, CancellationToken token)
            {
                for (var i = 0; i < count; i++)
                {
                    var operation = i % 2 == 0
                        ? integrationTransport.IntegrationContext.Send(new TestCommand(i), token)
                        : integrationTransport.IntegrationContext.Publish(new TestEvent(i), token);

                    await operation.ConfigureAwait(false);
                }
            }
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task ThrowingMessageHandlerTest(IIntegrationTransport transport)
        {
            var incomingMessages = new ConcurrentBag<IntegrationMessage>();
            var actualRefusedMessagesCount = 0;

            transport.OnMessage += (_, args) =>
            {
                incomingMessages.Add(args.GeneralMessage);
            };

            transport.OnError += (_, _) =>
            {
                Interlocked.Increment(ref actualRefusedMessagesCount);
            };

            var assembly = typeof(IExecutableEndpoint).Assembly; // GenericEndpoint.Executable

            var additionalTypes = new[]
            {
                typeof(TestCommand),
                typeof(ThrowingMessageHandler)
            };

            var endpointIdentity = new EndpointIdentity(nameof(ThrowingMessageHandlerTest), nameof(ThrowingMessageHandlerTest));
            var identityRegistration = Fixture.DelegateRegistration(container => container.RegisterInstance(typeof(EndpointIdentity), endpointIdentity));
            var options = new DependencyContainerOptions()
                .WithManualRegistration(identityRegistration)
                .WithManualRegistration(((IAdvancedIntegrationTransport)transport).Injection)
                .WithManualRegistration(new CrossCuttingConcernsManualRegistration())
                .WithManualRegistration(new LoggerTestRegistration());

            var boundedContainer = Fixture.BoundedAboveContainer(options, assembly);
            var typeProvider = new TypeProviderMock(boundedContainer.Resolve<ITypeProvider>(), additionalTypes);
            var retryPolicy = new RetryPolicyMock();
            var overrides = Fixture.DelegateRegistration(container =>
            {
                container
                    .RegisterInstance(typeof(ITypeProvider), typeProvider)
                    .RegisterInstance(typeProvider.GetType(), typeProvider)
                    .RegisterInstance(typeof(IRetryPolicy), retryPolicy)
                    .RegisterInstance(retryPolicy.GetType(), retryPolicy);
            });

            var handlersRegistration = Fixture.DelegateRegistration(container =>
            {
                container.Register(typeof(IMessageHandler<>), typeof(ThrowingMessageHandler));
            });

            var containerOptions = new DependencyContainerOptions()
                .WithManualRegistration(handlersRegistration)
                .WithOverride(overrides);

            var endpointOptions = new EndpointOptions(new EndpointIdentity(Endpoint1, 0), containerOptions,  assembly);

            IReadOnlyCollection<FailedMessage> failedMessages;

            using (var host = Host.CreateDefaultBuilder().ConfigureHost(transport, endpointOptions).Build())
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await transport.IntegrationContext.Send(new TestCommand(42), cts.Token).ConfigureAwait(false);

                await Task.Delay(TimeSpan.FromSeconds(4), cts.Token).ConfigureAwait(false);

                failedMessages = host.Services.GetRequiredService<IHostStatistics>().FailedMessages;

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }

            Output.WriteLine($"{nameof(incomingMessages)}Count: {incomingMessages.Count}");
            Output.WriteLine(incomingMessages.Select((message, index) => $"[{index}] - {message}").ToString(Environment.NewLine));

            Assert.Equal(4, incomingMessages.Count);
            Assert.Single(incomingMessages.Select(it => it.ReflectedType).Distinct());
            Assert.Single(incomingMessages.Select(it => it.Payload.ToString()).Distinct());
            Assert.Single(incomingMessages.Select(it => it.ReadRequiredHeader<Guid>(IntegratedMessageHeader.ConversationId)).Distinct());

            var actualRetryIndexes = incomingMessages
                .Select(it => it.ReadHeader<int>(IntegratedMessageHeader.RetryCounter))
                .OrderBy(it => it)
                .ToList();
            Assert.Equal(new List<int> { 0, 1, 2, 3 }, actualRetryIndexes);

            Output.WriteLine($"{nameof(actualRefusedMessagesCount)}: {actualRefusedMessagesCount}");
            Assert.Equal(1, actualRefusedMessagesCount);

            Output.WriteLine($"{nameof(IHostStatistics.FailedMessages)}Count: {failedMessages.Count}");
            Assert.Single(failedMessages);
            var failedMessage = failedMessages.Single();
            Output.WriteLine(failedMessage.ToString());
            var exception = failedMessage.Exception;
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal("42", exception.Message);
            Assert.Equal("42", failedMessage.Message.Payload.ToString());
        }

        [Component(EnLifestyle.Transient)]
        private class ThrowingMessageHandler : IMessageHandler<TestCommand>
        {
            public Task Handle(TestCommand message, IIntegrationContext context, CancellationToken token)
            {
                throw new InvalidOperationException(message.Id.ToString(CultureInfo.InvariantCulture));
            }
        }

        [Component(EnLifestyle.Transient)]
        private class TestMessageHandler : IMessageHandler<TestCommand>, IMessageHandler<TestEvent>, IMessageHandler<TestQuery>
        {
            public Task Handle(TestCommand message, IIntegrationContext context, CancellationToken token)
            {
                return Task.CompletedTask;
            }

            public Task Handle(TestEvent message, IIntegrationContext context, CancellationToken token)
            {
                return Task.CompletedTask;
            }

            public Task Handle(TestQuery message, IIntegrationContext context, CancellationToken token)
            {
                return message.Id % 2 == 0
                    ? Task.CompletedTask
                    : context.Reply(message, new TestReply(message.Id), token);
            }
        }

        [OwnedBy(Endpoint1)]
        private class TestCommand : IIntegrationCommand
        {
            public TestCommand(int id)
            {
                Id = id;
            }

            internal int Id { get; }

            public override string ToString()
            {
                return Id.ToString(CultureInfo.InvariantCulture);
            }
        }

        [OwnedBy(Endpoint1)]
        private class TestQuery : IIntegrationQuery<TestReply>
        {
            public TestQuery(int id)
            {
                Id = id;
            }

            internal int Id { get; }

            public override string ToString()
            {
                return Id.ToString(CultureInfo.InvariantCulture);
            }
        }

        private class TestReply : IIntegrationMessage
        {
            public TestReply(int id)
            {
                Id = id;
            }

            internal int Id { get; }

            public override string ToString()
            {
                return Id.ToString(CultureInfo.InvariantCulture);
            }
        }

        [OwnedBy(Endpoint2)]
        private class TestEvent : IIntegrationEvent
        {
            public TestEvent(int id)
            {
                Id = id;
            }

            internal int Id { get; }

            public override string ToString()
            {
                return Id.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}