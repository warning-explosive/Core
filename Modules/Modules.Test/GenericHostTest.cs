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

            var endpointIdentity = new EndpointIdentity(nameof(IntegrationContextDecoratorTest), 0);
            var integrationMessage = new IntegrationMessage(new TestCommand(0), typeof(TestCommand));
            var endpointRuntimeInfo = new EndpointRuntimeInfo(endpointIdentity, integrationMessage);

            Assert.Throws<InvalidOperationException>(() => transport.DependencyContainer.Resolve<IExtendedIntegrationContext>());
            var extendedIntegrationContext = transport.DependencyContainer.Resolve<IExtendedIntegrationContext, EndpointRuntimeInfo>(endpointRuntimeInfo);
            Assert.Throws<SimpleInjector.ActivationException>(() => transport.DependencyContainer.Resolve<IIntegrationContext>());
            var ubiquitousIntegrationContext = transport.DependencyContainer.Resolve<IUbiquitousIntegrationContext>();

            Assert.NotNull(extendedIntegrationContext);
            Assert.NotNull(ubiquitousIntegrationContext);

            var assemblyName = AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericHost));
            var requirementsDecoratorTypeFullName = AssembliesExtensions.BuildName(assemblyName, "Internals", "ExtendedIntegrationContextRequirementsDecorator");
            var headersMaintenanceDecoratorTypeFullName = AssembliesExtensions.BuildName(assemblyName, "Internals", "ExtendedIntegrationContextHeadersMaintenanceDecorator");
            var requirementsDecoratorType = AssembliesExtensions.FindRequiredType(assemblyName, requirementsDecoratorTypeFullName);
            var headersMaintenanceDecoratorType = AssembliesExtensions.FindRequiredType(assemblyName, headersMaintenanceDecoratorTypeFullName);
            var inMemoryIntegrationContextTypeFullName = AssembliesExtensions.BuildName(assemblyName, "Transport", "InMemoryIntegrationContext");
            var integrationsContextType = AssembliesExtensions.FindRequiredType(assemblyName, inMemoryIntegrationContextTypeFullName);
            var expected = new[] { requirementsDecoratorType, headersMaintenanceDecoratorType, integrationsContextType };

            Assert.True(expected.SequenceEqual(extendedIntegrationContext.ExtractDecorators().ShowTypes(nameof(IExtendedIntegrationContext), Output.WriteLine)));
            Assert.Equal(integrationsContextType, ubiquitousIntegrationContext.ExtractDecorators().Single());
        }

        [Fact]
        internal void TestExtensionTest()
        {
            var manualRegistrations = new IManualRegistration[]
            {
                new DelegatesRegistration(),
                new VersionedOpenGenericRegistration(),
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
                    Assembly = GetType().Assembly,
                    ContainerOptions = new DependencyContainerOptions
                    {
                        ManualRegistrations = new IManualRegistration[]
                        {
                            new DelegatesRegistration(),
                            new VersionedOpenGenericRegistration(),
                        }
                    }
                };

                return Host
                    .CreateDefaultBuilder()
                    .ConfigureHost(transport, endpointOptions)
                    .Build();
            }
        }

        [Fact]
        internal async Task SimpleHostTest()
        {
            using var cts = new CancellationTokenSource();

            var transport = GenericHost.InMemoryIntegrationTransport(new InMemoryIntegrationTransportOptions());
            var totalCount = 0;
            transport.OnMessage += (_, args) =>
            {
                Output.WriteLine($"{Interlocked.Increment(ref totalCount)}: {args.GeneralMessage}");
            };

            var assembly = GetType().Assembly;

            DependencyContainerOptions ContainerOptions()
            {
                var registrations = new IManualRegistration[]
                {
                    new DelegatesRegistration(),
                    new VersionedOpenGenericRegistration(),
                };

                return new DependencyContainerOptions
                {
                    ManualRegistrations = registrations
                };
            }

            var options10 = new EndpointOptions(new EndpointIdentity(Endpoint1, 0), transport) { Assembly = assembly, ContainerOptions = ContainerOptions() };
            var options11 = new EndpointOptions(new EndpointIdentity(Endpoint1, 1), transport) { Assembly = assembly, ContainerOptions = ContainerOptions() };
            var options20 = new EndpointOptions(new EndpointIdentity(Endpoint2, 0), transport) { Assembly = assembly, ContainerOptions = ContainerOptions() };

            using var host = Host
                .CreateDefaultBuilder()
                .ConfigureHost(transport, options10, options11, options20)
                .Build();

            var runningHost = Task.Run(async () => await host.RunAsync(cts.Token).ConfigureAwait(false), cts.Token);
            var messagesCount = 1000;
            var backgroundInitiatorTask = SendAndPublish(transport, messagesCount);

            await backgroundInitiatorTask.ConfigureAwait(false);

            cts.Cancel();
            await runningHost.ConfigureAwait(false);

            Output.WriteLine($"{nameof(totalCount)}: {totalCount}");
            Assert.Equal(messagesCount, totalCount);
        }

        private static Task SendAndPublish(IIntegrationTransport transport, int messagesCount)
        {
            return Task.Run(async () =>
            {
                var ctx = transport.DependencyContainer.Resolve<IUbiquitousIntegrationContext>();

                for (var i = 0; i < messagesCount; ++i)
                {
                    if (i % 2 == 0)
                    {
                        await ctx.Send(new TestCommand(i), CancellationToken.None).ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.Publish(new TestEvent(i), CancellationToken.None).ConfigureAwait(false);
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
                }
            });
        }

        [Lifestyle(EnLifestyle.Transient)]
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