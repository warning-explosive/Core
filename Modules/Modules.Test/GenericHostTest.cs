namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics.Test;
    using ClassFixtures;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;
    using GenericHost;
    using GenericHost.Abstractions;
    using GenericHost.Defaults;
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
            {
                var registrations = new IManualRegistration[]
                {
                    new DelegatesRegistration(),
                    new VersionedOpenGenericRegistration()
                };

                return new DependencyContainerOptions
                {
                    ManualRegistrations = registrations
                };
            }

            var options10 = new EndpointOptions(new EndpointIdentity(Endpoint1, 0)) { Assembly = assembly, ContainerOptions = ContainerOptions() };
            var options11 = new EndpointOptions(new EndpointIdentity(Endpoint1, 1)) { Assembly = assembly, ContainerOptions = ContainerOptions() };
            var options20 = new EndpointOptions(new EndpointIdentity(Endpoint2, 0)) { Assembly = assembly, ContainerOptions = ContainerOptions() };

            var compositeEndpoint = await Endpoint.StartAsync(cts.Token, options10, options11, options20).ConfigureAwait(false);

            var transport = GenericHost.InMemoryTransport(new DefaultEndpointInstanceSelectionBehavior());
            var totalCount = 0;
            transport.OnMessage += (_, args) =>
            {
                Output.WriteLine($"{Interlocked.Increment(ref totalCount)}: {args.GeneralMessage}");
            };

            using var host = Host.CreateDefaultBuilder()
                .UseTransport(transport, compositeEndpoint)
                .Build();

            var runningHost = Task.Run(async () => await host.RunAsync(cts.Token).ConfigureAwait(false), cts.Token);
            var backgroundInitiatorTask = SendAndPublish(transport);

            await backgroundInitiatorTask.ConfigureAwait(false);

            cts.Cancel();
            await runningHost.ConfigureAwait(false);
        }

        private static Task SendAndPublish(IIntegrationTransport transport)
        {
            return Task.Run(async () =>
            {
                var ctx = transport.CreateContext();

                for (var i = 0; i < 100; ++i)
                {
                    if (i % 3 == 0)
                    {
                        await ctx.Send(new TestCommand(i), CancellationToken.None).ConfigureAwait(false);
                    }
                    else if (i % 3 == 1)
                    {
                        await ctx.Request<TestQuery, TestQueryResponse>(new TestQuery(i), CancellationToken.None).ConfigureAwait(false);
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
                return context.Reply(message, new TestQueryResponse(), token);
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
        private class TestQuery : IIntegrationQuery<TestQueryResponse>
        {
            public TestQuery(int id)
            {
                Id = id;
            }

            private int Id { get; }

            public override string ToString()
            {
                return Id.ToString(CultureInfo.InvariantCulture);
            }
        }

        private class TestQueryResponse : IIntegrationMessage
        {
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