namespace SpaceEngineers.Core.Modules.Test
{
    using System;
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
    using GenericHost.InMemoryIntegrationTransport;
    using Microsoft.Extensions.Hosting;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// GenericHost assembly tests
    /// </summary>
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

        [Fact]
        internal async Task IntegrationContextDecoratorTest()
        {
            var transport = Transport.InMemoryIntegrationTransport();
            var dependencyContainer = transport.GetFieldValue<IDependencyContainer>("_dependencyContainer");
            var integrationMessage = new IntegrationMessage(new TestCommand(0), typeof(TestCommand));

            // 1 - IUbiquitousIntegrationContext
            Assert.Throws<InvalidOperationException>(() => dependencyContainer.Resolve<IExtendedIntegrationContext>());
            Assert.Throws<SimpleInjector.ActivationException>(() => dependencyContainer.Resolve<IExtendedIntegrationContext, IntegrationMessage>(integrationMessage));
            Assert.Throws<SimpleInjector.ActivationException>(() => dependencyContainer.Resolve<IIntegrationContext>());
            var ubiquitousIntegrationContext = dependencyContainer.Resolve<IUbiquitousIntegrationContext>();

            Assert.NotNull(ubiquitousIntegrationContext);

            var assemblyName = AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericHost), nameof(Core.GenericHost.InMemoryIntegrationTransport));
            var inMemoryUbiquitousIntegrationContextTypeFullName = AssembliesExtensions.BuildName(assemblyName, "Internals", "InMemoryUbiquitousIntegrationContext");
            var integrationContextType = AssembliesExtensions.FindRequiredType(assemblyName, inMemoryUbiquitousIntegrationContextTypeFullName);

            var decorators = ubiquitousIntegrationContext.ExtractDecorators().ToList();
            Assert.Single(decorators);

            var actual = decorators.ShowTypes(nameof(IUbiquitousIntegrationContext), Output.WriteLine).Single();
            Assert.Equal(integrationContextType, actual);

            // 2 - IExtendedIntegrationContext
            var endpointIdentity = new EndpointIdentity(Endpoint1, 0);
            var endpointOptions = new EndpointOptions(endpointIdentity, typeof(IExecutableEndpoint).Assembly);

            using (var host = Host.CreateDefaultBuilder().ConfigureHost(transport, endpointOptions).Build())
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                /* TODO: assert running endpoint and check IExtendedIntegrationContext */

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }

        [Fact]
        internal void TestExtensionTest()
        {
            var transport = Transport.InMemoryIntegrationTransport();

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
                transport.EndpointInjection,
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

        [Fact]
        internal async Task StartStopRunTest()
        {
            var noneToken = CancellationToken.None;

            using (var startStopHost = BuildHost())
            {
                await startStopHost.StartAsync(noneToken).ConfigureAwait(false);
                await startStopHost.StopAsync(noneToken).ConfigureAwait(false);
            }

            using (var runHost = BuildHost())
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await runHost.RunAsync(cts.Token).ConfigureAwait(false);
            }

            IHost BuildHost()
            {
                var transport = Transport.InMemoryIntegrationTransport();

                var endpointIdentity = new EndpointIdentity(Endpoint1, 0);
                var endpointOptions = new EndpointOptions(endpointIdentity, typeof(IExecutableEndpoint).Assembly);

                return Host
                    .CreateDefaultBuilder()
                    .ConfigureHost(transport, endpointOptions)
                    .Build();
            }
        }

        [Fact]
        internal void InMemoryIntegrationTransportBuildTest()
        {
            var transport = Transport.InMemoryIntegrationTransport();
            var dependencyContainer = transport.GetFieldValue<IDependencyContainer>("_dependencyContainer");
            Assert.NotNull(dependencyContainer);
            Assert.Same(transport, dependencyContainer.Resolve<IIntegrationTransport>());
            Assert.Same(transport.IntegrationContext, dependencyContainer.Resolve<IUbiquitousIntegrationContext>());
        }

        [Fact(Timeout = 300_000)]
        internal async Task SimpleHostTest()
        {
            var expectedCount = 1000;
            var actualCount = 0;

            var transport = Transport.InMemoryIntegrationTransport();
            transport.OnMessage += (_, _) =>
            {
                Interlocked.Increment(ref actualCount);
            };

            var assembly = typeof(IExecutableEndpoint).Assembly;

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

            using var host = Host
                .CreateDefaultBuilder()
                .ConfigureHost(transport, options10, options11, options20)
                .Build();

            using var cts = new CancellationTokenSource();

            var runningHost = Task.Run(async () => await host.RunAsync(cts.Token).ConfigureAwait(false), cts.Token);

            await SendAndPublishInBackground(transport, expectedCount).ConfigureAwait(false);

            cts.Cancel();

            await runningHost.ConfigureAwait(false);

            Output.WriteLine($"{nameof(actualCount)}: {actualCount}");
            Assert.Equal(expectedCount, actualCount);

            /* TODO: assert successful delivery to endpoints */
        }

        private static Task SendAndPublishInBackground(IIntegrationTransport transport, int messagesCount)
        {
            return Task.Run(async () =>
            {
                for (var i = 0; i < messagesCount; ++i)
                {
                    if (i % 2 == 0)
                    {
                        await transport.IntegrationContext.Send(new TestCommand(i), CancellationToken.None).ConfigureAwait(false);
                    }
                    else
                    {
                        await transport.IntegrationContext.Publish(new TestEvent(i), CancellationToken.None).ConfigureAwait(false);
                    }
                }
            });
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