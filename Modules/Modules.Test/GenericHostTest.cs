namespace SpaceEngineers.Core.Modules.Test
{
    using System;
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
    using Basics;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
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
        internal async Task IntegrationContextCheckAppliedDecoratorTest(IIntegrationTransport transport)
        {
            var transportDependencyContainer = transport.GetPropertyValue<IDependencyContainer>("DependencyContainer");
            var integrationMessage = new IntegrationMessage(new TestCommand(0), typeof(TestCommand));

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
            var endpointIdentity = new EndpointIdentity(Endpoint1, 0);
            var assembly = typeof(IExecutableEndpoint).Assembly; // GenericEndpoint.Executable
            var endpointOptions = new EndpointOptions(endpointIdentity, assembly);

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            using (var host = Host.CreateDefaultBuilder().ConfigureHost(transport, endpointOptions).Build())
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var hostedService = (IHostedService?)host.Services.GetService(typeof(IHostedService));
                Assert.NotNull(hostedService);
                var endpoints = hostedService!.GetPropertyValue<IReadOnlyCollection<IGenericEndpoint>>("Endpoints");
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

            var manualRegistrations = new IManualRegistration[]
            {
                ((IAdvancedIntegrationTransport)transport).Injection,
                endpointIdentityRegistration,
                handlersRegistration
            };

            var options = new DependencyContainerOptions
            {
                ManualRegistrations = manualRegistrations
            };

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
                .ShouldNotRequest<TestQuery, TestQueryReply>()
                .Replied<TestQueryReply>(reply => reply.Id == 43)
                .Invoke();

            MessageHandlerTestBuilder<T> ShouldNotProduceMessages<T>(MessageHandlerTestBuilder<T> builder)
                where T : IIntegrationMessage
            {
                return builder
                    .ShouldProduceNothing()
                    .ShouldNotSend<IIntegrationCommand>()
                    .ShouldNotPublish<IIntegrationEvent>()
                    .ShouldNotRequest<TestQuery, TestQueryReply>()
                    .ShouldNotReply<IIntegrationMessage>();
            }
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task RunTest(IIntegrationTransport transport)
        {
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            using (var runnableHost = BuildHost())
            {
                await runnableHost.RunAsync(cts.Token).ConfigureAwait(false);
            }

            IHost BuildHost()
            {
                var endpointIdentity = new EndpointIdentity(Endpoint1, 0);
                var assembly = typeof(IExecutableEndpoint).Assembly; // GenericEndpoint.Executable
                var endpointOptions = new EndpointOptions(endpointIdentity, assembly);

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
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            using (var startStopHost = BuildHost())
            {
                await startStopHost.StartAsync(cts.Token).ConfigureAwait(false);
                await startStopHost.StopAsync(cts.Token).ConfigureAwait(false);
            }

            IHost BuildHost()
            {
                var endpointIdentity = new EndpointIdentity(Endpoint1, 0);
                var assembly = typeof(IExecutableEndpoint).Assembly; // GenericEndpoint.Executable
                var endpointOptions = new EndpointOptions(endpointIdentity, assembly);

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

            transport.OnMessage += (_, _) =>
            {
                Interlocked.Increment(ref actualMessagesCount);
            };

            var assembly = typeof(IExecutableEndpoint).Assembly; // GenericEndpoint.Executable

            var handlersRegistration = Fixture.DelegateRegistration(container =>
            {
                container.Register(typeof(IMessageHandler<>), typeof(TestMessageHandler));
            });

            var getContainerOptions = new Func<DependencyContainerOptions>(() =>
            {
                return new DependencyContainerOptions
                {
                    ManualRegistrations = new[]
                    {
                        handlersRegistration
                    }
                };
            });

            var options10 = new EndpointOptions(new EndpointIdentity(Endpoint1, 0), assembly) { ContainerOptions = getContainerOptions() };
            var options11 = new EndpointOptions(new EndpointIdentity(Endpoint1, 1), assembly) { ContainerOptions = getContainerOptions() };
            var options20 = new EndpointOptions(new EndpointIdentity(Endpoint2, 0), assembly) { ContainerOptions = getContainerOptions() };

            IReadOnlyCollection<Exception> dispatchingErrors;

            using (var host = Host.CreateDefaultBuilder().ConfigureHost(transport, options10, options11, options20).Build())
            using (var cts = new CancellationTokenSource())
            {
                var runningHost = InBackground(token => RunHost(host, token), cts.Token);

                await InBackground(token => SendInitiationMessages(transport, expectedMessagesCount, token), cts.Token).ConfigureAwait(false);

                dispatchingErrors = host.Services.GetRequiredService<IHostStatistics>().DispatchingErrors;

                cts.Cancel();

                await runningHost.ConfigureAwait(false);
            }

            Output.WriteLine($"{nameof(actualMessagesCount)}: {actualMessagesCount}");
            Assert.Equal(expectedMessagesCount, actualMessagesCount);

            Output.WriteLine($"{nameof(IHostStatistics.DispatchingErrors)}Count: {dispatchingErrors.Count}");
            if (dispatchingErrors.Count > 0) { Output.WriteLine(dispatchingErrors.First().ToString()); }
            Assert.Equal(0, dispatchingErrors.Count);

            /* TODO: assert error queue */

            static Task InBackground(Func<CancellationToken, Task> func, CancellationToken token)
            {
                return Task.Run(() => func(token), token);
            }

            Task RunHost(IHost host, CancellationToken token)
            {
                return host.RunAsync(token);
            }

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
                    : context.Reply(message, new TestQueryReply(message.Id), token);
            }
        }

        [OwnedBy(Endpoint1)]
        private class TestCommand : IIntegrationCommand
        {
            public TestCommand(int id)
            {
                Id = id;
            }

            private int Id { get; }

            public override string ToString()
            {
                return Id.ToString(CultureInfo.InvariantCulture);
            }
        }

        [OwnedBy(Endpoint1)]
        private class TestQuery : IIntegrationQuery<TestQueryReply>
        {
            public TestQuery(int id)
            {
                Id = id;
            }

            public int Id { get; }

            public override string ToString()
            {
                return Id.ToString(CultureInfo.InvariantCulture);
            }
        }

        private class TestQueryReply : IIntegrationMessage
        {
            public TestQueryReply(int id)
            {
                Id = id;
            }

            internal int Id { get; }
        }

        [OwnedBy(Endpoint2)]
        private class TestEvent : IIntegrationEvent
        {
            public TestEvent(int id)
            {
                Id = id;
            }

            private int Id { get; }

            public override string ToString()
            {
                return Id.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}