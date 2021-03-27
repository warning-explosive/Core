namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Basics.Test;
    using Core.Test.Api.ClassFixtures;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;
    using GenericEndpoint.TestExtensions;
    using GenericHost;
    using GenericHost.Abstractions;
    using Microsoft.Extensions.Hosting;
    using Registrations;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// GenericHost assembly tests
    /// </summary>
    public class GenericHostTest : BasicsTestBase, IClassFixture<ModulesTestFixture>
    {
        private const string Endpoint1 = nameof(Endpoint1);
        private const string Endpoint2 = nameof(Endpoint2);

        private readonly ModulesTestFixture _fixture;

        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public GenericHostTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output)
        {
            _fixture = fixture;
        }

        [Fact]
        internal void IntegrationContextDecoratorTest()
        {
            var transport = GenericHost.InMemoryIntegrationTransport(new InMemoryIntegrationTransportOptions());
            var integrationMessage = new IntegrationMessage(new TestCommand(0), typeof(TestCommand));

            Assert.Throws<InvalidOperationException>(() => GetDependencyContainer().Resolve<IExtendedIntegrationContext>());
            Assert.Throws<SimpleInjector.ActivationException>(() => GetDependencyContainer().Resolve<IExtendedIntegrationContext, IntegrationMessage>(integrationMessage));
            Assert.Throws<SimpleInjector.ActivationException>(() => GetDependencyContainer().Resolve<IIntegrationContext>());
            var ubiquitousIntegrationContext = GetDependencyContainer().Resolve<IUbiquitousIntegrationContext>();

            Assert.NotNull(ubiquitousIntegrationContext);

            var assemblyName = AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericHost));
            var inMemoryUbiquitousIntegrationContextTypeFullName = AssembliesExtensions.BuildName(assemblyName, "Transport", "InMemoryUbiquitousIntegrationContext");
            var integrationContextType = AssembliesExtensions.FindRequiredType(assemblyName, inMemoryUbiquitousIntegrationContextTypeFullName);

            Assert.Equal(integrationContextType, ubiquitousIntegrationContext.ExtractDecorators().Single());

            IDependencyContainer GetDependencyContainer()
            {
                return transport.GetFieldValue<IDependencyContainer>("_dependencyContainer");
            }
        }

        [Fact]
        internal void TestExtensionTest()
        {
            var manualRegistrations = new IManualRegistration[]
            {
                new GenericEndpointRegistration(),
                GenericHost.InMemoryIntegrationTransport(new InMemoryIntegrationTransportOptions()).Registration
            };

            var dependencyContainer = _fixture
                .GetDependencyContainer(
                    GetType().Assembly,
                    new[] { typeof(GenericHost).Assembly },
                    manualRegistrations);

            ShouldNotProduceMessages(dependencyContainer.Resolve<IMessageHandler<TestCommand>>().OnMessage(new TestCommand(42))).Invoke();
            ShouldNotProduceMessages(dependencyContainer.Resolve<IMessageHandler<TestEvent>>().OnMessage(new TestEvent(42))).Invoke();
            ShouldNotProduceMessages(dependencyContainer.Resolve<IMessageHandler<TestQuery>>().OnMessage(new TestQuery(42))).Invoke();

            dependencyContainer
                .Resolve<IMessageHandler<TestQuery>>()
                .OnMessage(new TestQuery(43)).ShouldNotSend<IIntegrationCommand>()
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
                var transport = GenericHost.InMemoryIntegrationTransport(new InMemoryIntegrationTransportOptions());

                var endpointOptions = new EndpointOptions(new EndpointIdentity(Endpoint1, 0), transport)
                {
                    Assembly = GetType().Assembly
                };

                return Host
                    .CreateDefaultBuilder()
                    .ConfigureHost(transport, endpointOptions)
                    .Build();
            }
        }

        [Fact(Timeout = 300_000)]
        internal async Task SimpleHostTest()
        {
            var expectedCount = 1000;
            var actualCount = 0;

            var transport = GenericHost.InMemoryIntegrationTransport(new InMemoryIntegrationTransportOptions());
            transport.OnMessage += (_, _) =>
            {
                Interlocked.Increment(ref actualCount);
            };

            var assembly = GetType().Assembly;

            var options10 = new EndpointOptions(new EndpointIdentity(Endpoint1, 0), transport) { Assembly = assembly };
            var options11 = new EndpointOptions(new EndpointIdentity(Endpoint1, 1), transport) { Assembly = assembly };
            var options20 = new EndpointOptions(new EndpointIdentity(Endpoint2, 0), transport) { Assembly = assembly };

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