namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics.Test;
    using ClassFixtures;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Attributes;
    using GenericHost;
    using GenericHost.Abstractions;
    using GenericHost.Implementations;
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
        internal async Task SimpleHostTest()
        {
            using var cts = new CancellationTokenSource();

            var assembly = GetType().Assembly;

            DependencyContainerOptions ContainerOptions()
                => _fixture.GetDependencyContainerOptions(typeof(EndpointIdentityRegistration));

            var options10 = new EndpointOptions(new EndpointIdentity(Endpoint1, 0)) { Assembly = assembly, ContainerOptions = ContainerOptions() };
            var options11 = new EndpointOptions(new EndpointIdentity(Endpoint1, 1)) { Assembly = assembly, ContainerOptions = ContainerOptions() };
            var options20 = new EndpointOptions(new EndpointIdentity(Endpoint2, 0)) { Assembly = assembly, ContainerOptions = ContainerOptions() };

            var compositeEndpoint = await Endpoint.StartAsync(cts.Token, options10, options11, options20).ConfigureAwait(false);

            var transport = new InMemoryIntegrationTransport(new DefaultEndpointInstanceSelectionBehavior());
            transport.OnMessage += (_, e) =>
            {
                Output.WriteLine(e.ToString());
            };

            using var transportHost = Host.CreateDefaultBuilder()
                .UseTransport(transport, compositeEndpoint)
                .Build();

            /*
            TODO: run host, calculate messages, wait graceful shutdown

            SendAndPublish(transport);

            cts.CancelAfter(TimeSpan.FromSeconds(10));

            await Assert.ThrowsAsync<TaskCanceledException>(() => transportHost.RunAsync(cts.Token)).ConfigureAwait(false);
            */
        }

        private static void SendAndPublish(IIntegrationTransport transport)
        {
            Task.Run(async () =>
            {
                var ctx = transport.CreateContext();

                for (var i = 0; i < 100; ++i)
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
        private class TestMessageHandler : IMessageHandler<TestCommand>, IMessageHandler<TestEvent>
        {
            public Task Handle(TestCommand message, IIntegrationContext context, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            public Task Handle(TestEvent message, IIntegrationContext context, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
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