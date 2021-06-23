namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;
    using GenericEndpoint.Host;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.TestExtensions;
    using GenericHost;
    using GenericHost.Api.Abstractions;
    using InMemoryIntegrationTransport.Host;
    using IntegrationTransport.Api.Abstractions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Mocks;
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
            var useInMemoryIntegrationTransport = new Func<IHostBuilder, IHostBuilder>(hostBuilder => hostBuilder.UseInMemoryIntegrationTransport());
            var inMemoryIntegrationTransportAssembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.InMemoryIntegrationTransport), nameof(Core.InMemoryIntegrationTransport.Endpoint)));
            var assemblies = new[] { inMemoryIntegrationTransportAssembly };

            yield return new object[] { useInMemoryIntegrationTransport, assemblies };
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task ThrowingMessageHandlerTest(Func<IHostBuilder, IHostBuilder> useTransport, Assembly[] endpointPluginAssemblies)
        {
            var incomingMessages = new ConcurrentBag<IntegrationMessage>();
            var failedMessages = new ConcurrentBag<FailedMessage>();

            var actualRefusedMessagesCount = 0;

            var additionalTypes = new[]
            {
                typeof(TestCommand),
                typeof(ThrowingMessageHandler)
            };

            var endpointIdentity = new EndpointIdentity(Endpoint1, 0);

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseEndpoint(builder => builder
                    .WithEndpointPluginAssemblies(endpointPluginAssemblies)
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .WithMessageHandlers(typeof(ThrowingMessageHandler))
                    .ModifyContainerOptions(ExtendTypeProvider(additionalTypes))
                    .BuildOptions(endpointIdentity))
                .BuildHost();

            using (host)
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var integrationContext = host
                    .GetTransportDependencyContainer()
                    .Resolve<Core.GenericHost.Abstractions.IIntegrationContext>();

                await integrationContext.Send(new TestCommand(42), cts.Token).ConfigureAwait(false);

                await Task.Delay(TimeSpan.FromSeconds(4), cts.Token).ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }

            Output.WriteLine($"{nameof(incomingMessages)}Count: {incomingMessages.Count}");
            Output.WriteLine(incomingMessages.Select((message, index) => $"[{index}] - {message}").ToString(Environment.NewLine));

            Assert.Equal(4, incomingMessages.Count);
            Assert.Single(incomingMessages.Select(it => it.ReflectedType).Distinct());
            Assert.Single(incomingMessages.Select(it => it.Payload.ToString()).Distinct());
            Assert.Single(incomingMessages.Select(it => it.ReadRequiredHeader<Guid>(IntegrationMessageHeader.ConversationId)).Distinct());

            var actualRetryIndexes = incomingMessages
                .Select(it => it.ReadHeader<int>(IntegrationMessageHeader.RetryCounter))
                .OrderBy(it => it)
                .ToList();
            Assert.Equal(new List<int> { 0, 1, 2, 3 }, actualRetryIndexes);

            Output.WriteLine($"{nameof(actualRefusedMessagesCount)}: {actualRefusedMessagesCount}");
            Assert.Equal(1, actualRefusedMessagesCount);

            Assert.Single(failedMessages);
            var failedMessage = failedMessages.Single();
            Output.WriteLine(failedMessage.ToString());
            var exception = failedMessage.Exception;
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal("42", exception.Message);
            Assert.Equal("42", failedMessage.Message.Payload.ToString());
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task SimpleHostTest(Func<IHostBuilder, IHostBuilder> useTransport, Assembly[] endpointPluginAssemblies)
        {
            var expectedMessagesCount = 1000;
            var expectedRefusedMessagesCount = 0;

            var actualMessagesCount = 0;
            var actualRefusedMessagesCount = 0;

            var additionalTypes = new[]
            {
                typeof(TestCommand),
                typeof(TestEvent),
                typeof(TestQuery),
                typeof(TestReply),
                typeof(TestMessageHandler)
            };

            var endpointIdentity10 = new EndpointIdentity(Endpoint1, 0);
            var endpointIdentity11 = new EndpointIdentity(Endpoint1, 1);
            var endpointIdentity20 = new EndpointIdentity(Endpoint2, 0);

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseEndpoint(builder => builder
                    .WithEndpointPluginAssemblies(endpointPluginAssemblies)
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .WithMessageHandlers(typeof(TestMessageHandler))
                    .ModifyContainerOptions(ExtendTypeProvider(additionalTypes))
                    .BuildOptions(endpointIdentity10))
                .UseEndpoint(builder => builder
                    .WithEndpointPluginAssemblies(endpointPluginAssemblies)
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .WithMessageHandlers(typeof(TestMessageHandler))
                    .ModifyContainerOptions(ExtendTypeProvider(additionalTypes))
                    .BuildOptions(endpointIdentity11))
                .UseEndpoint(builder => builder
                    .WithEndpointPluginAssemblies(endpointPluginAssemblies)
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .WithMessageHandlers(typeof(TestMessageHandler))
                    .ModifyContainerOptions(ExtendTypeProvider(additionalTypes))
                    .BuildOptions(endpointIdentity20))
                .BuildHost();

            using (host)
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300)))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var integrationContext = host
                    .GetTransportDependencyContainer()
                    .Resolve<Core.GenericHost.Abstractions.IIntegrationContext>();

                await SendInitiationMessages(integrationContext, expectedMessagesCount, cts.Token).ConfigureAwait(false);

                await Task.Delay(TimeSpan.FromSeconds(3), cts.Token).ConfigureAwait(false);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }

            Output.WriteLine($"{nameof(actualMessagesCount)}: {actualMessagesCount}");
            Assert.Equal(expectedMessagesCount, actualMessagesCount);

            Output.WriteLine($"{nameof(actualRefusedMessagesCount)}: {actualRefusedMessagesCount}");
            Assert.Equal(expectedRefusedMessagesCount, actualRefusedMessagesCount);

            static async Task SendInitiationMessages(
                Core.GenericHost.Abstractions.IIntegrationContext integrationContext,
                int count,
                CancellationToken token)
            {
                for (var i = 0; i < count; i++)
                {
                    var operation = i % 2 == 0
                        ? integrationContext.Send(new TestCommand(i), token)
                        : integrationContext.Publish(new TestEvent(i), token);

                    await operation.ConfigureAwait(false);
                }
            }
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task SameTransportTest(Func<IHostBuilder, IHostBuilder> useTransport, Assembly[] endpointPluginAssemblies)
        {
            var identity1 = new EndpointIdentity(Endpoint1, 0);
            var identity2 = new EndpointIdentity(Endpoint2, 0);

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseEndpoint(builder => builder
                    .WithEndpointPluginAssemblies(endpointPluginAssemblies)
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .BuildOptions(identity1))
                .UseEndpoint(builder => builder
                    .WithEndpointPluginAssemblies(endpointPluginAssemblies)
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .BuildOptions(identity2))
                .BuildHost();

            bool transportIsSame;

            using (host)
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var transport = host.GetTransportDependencyContainer().Resolve<IIntegrationTransport>();

                transportIsSame = new[]
                    {
                        host.GetEndpointDependencyContainer(identity1),
                        host.GetEndpointDependencyContainer(identity2)
                    }
                    .Select(container => container.Resolve<IIntegrationTransport>())
                    .All(endpointTransport => ReferenceEquals(transport, endpointTransport));

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }

            Assert.True(transportIsSame);
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal void MessageHandlerTestExtensionsTest(Func<IHostBuilder, IHostBuilder> useTransport, Assembly[] endpointPluginAssemblies)
        {
            var endpointIdentity = new EndpointIdentity(nameof(MessageHandlerTestExtensionsTest), 0);

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseEndpoint(builder => builder
                    .WithEndpointPluginAssemblies(endpointPluginAssemblies)
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .WithMessageHandlers(typeof(TestMessageHandler))
                    .BuildOptions(endpointIdentity))
                .BuildHost();

            var dependencyContainer = host.GetEndpointDependencyContainer(endpointIdentity);

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

            TestMessageHandlerBuilder<T> ShouldNotProduceMessages<T>(TestMessageHandlerBuilder<T> builder)
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
        internal void BuildHostTest(Func<IHostBuilder, IHostBuilder> useTransport, Assembly[] endpointPluginAssemblies)
        {
            var endpointIdentity = new EndpointIdentity(nameof(BuildHostTest), 0);

            var testHost = useTransport(Host.CreateDefaultBuilder())
                .UseEndpoint(builder => builder
                    .WithEndpointPluginAssemblies(endpointPluginAssemblies)
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .BuildOptions(endpointIdentity))
                .BuildHost();

            using (testHost)
            {
                CheckHost(testHost);
                CheckEndpoint(testHost);
                CheckTransport(testHost);
            }

            void CheckHost(IHost host)
            {
                _ = host.Services.GetRequiredService<IHostedService>();

                var hostStartupActions = host
                    .Services
                    .GetServices<IHostStartupAction>()
                    .ToList();

                Assert.Single(hostStartupActions);
                var hostStartupAction = hostStartupActions.Single();

                Assert.Equal(typeof(SpaceEngineers.Core.GenericEndpoint.Host.Internals.GenericEndpointHostStartupAction), hostStartupAction.GetType());

                var hostBackgroundWorkers = host
                    .Services
                    .GetServices<IHostBackgroundWorker>()
                    .ToList();

                Assert.Single(hostBackgroundWorkers);
                var hostBackgroundWorker = hostBackgroundWorkers.Single();

                Assert.Equal(typeof(SpaceEngineers.Core.InMemoryIntegrationTransport.Host.Internals.InMemoryIntegrationTransportHostBackgroundWorker), hostBackgroundWorker.GetType());
            }

            void CheckEndpoint(IHost host)
            {
                var endpointDependencyContainer = host.GetEndpointDependencyContainer(endpointIdentity);
                var integrationMessage = new IntegrationMessage(new TestCommand(0), typeof(TestCommand), new StringFormatterMock());

                Assert.Throws<SimpleInjector.ActivationException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                Assert.Throws<SimpleInjector.ActivationException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage));
                Assert.Throws<SimpleInjector.ActivationException>(() => endpointDependencyContainer.Resolve<Core.GenericEndpoint.Api.Abstractions.IIntegrationContext>());

                using (endpointDependencyContainer.OpenScope())
                {
                    Assert.Throws<SimpleInjector.ActivationException>(() => endpointDependencyContainer.Resolve<IAdvancedIntegrationContext>());
                    var advancedIntegrationContext = endpointDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(integrationMessage);

                    var expected = new[]
                    {
                        typeof(SpaceEngineers.Core.GenericEndpoint.Internals.AdvancedIntegrationContextHeadersMaintenanceDecorator),
                        typeof(SpaceEngineers.Core.InMemoryIntegrationTransport.Endpoint.Internals.InMemoryIntegrationContext)
                    };

                    var actual = advancedIntegrationContext
                        .UnwrapDecorators()
                        .ShowTypes("#extended integration context", Output.WriteLine)
                        .ToList();

                    Assert.Equal(expected, actual);

                    var expectedPipeline = new[]
                    {
                        typeof(SpaceEngineers.Core.GenericEndpoint.Pipeline.ErrorHandlingPipeline),
                        typeof(SpaceEngineers.Core.GenericEndpoint.Statistics.Internals.StatisticsPipeline),
                        typeof(SpaceEngineers.Core.GenericEndpoint.Pipeline.UnitOfWorkPipeline),
                        typeof(SpaceEngineers.Core.GenericEndpoint.Pipeline.QueryReplyValidationPipeline),
                        typeof(SpaceEngineers.Core.GenericEndpoint.Pipeline.MessagePipeline),
                    };

                    var actualPipeline = endpointDependencyContainer
                        .Resolve<IMessagePipeline>()
                        .UnwrapDecorators()
                        .ShowTypes("#message pipeline", Output.WriteLine)
                        .ToList();

                    Assert.Equal(expectedPipeline, actualPipeline);
                }
            }

            void CheckTransport(IHost host)
            {
                var transportDependencyContainer = host.GetTransportDependencyContainer();

                _ = transportDependencyContainer.Resolve<IIntegrationTransport>();
                var integrationContext = transportDependencyContainer.Resolve<Core.GenericHost.Abstractions.IIntegrationContext>();

                var expected = new[]
                {
                    typeof(SpaceEngineers.Core.InMemoryIntegrationTransport.Host.Internals.InMemoryIntegrationContext)
                };

                var actual = integrationContext
                    .UnwrapDecorators()
                    .ShowTypes("#transport integration context", Output.WriteLine)
                    .ToList();

                Assert.Equal(expected, actual);
            }
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task RunTest(Func<IHostBuilder, IHostBuilder> useTransport, Assembly[] endpointPluginAssemblies)
        {
            var host = useTransport(Host.CreateDefaultBuilder())
                .UseEndpoint(builder => builder
                    .WithEndpointPluginAssemblies(endpointPluginAssemblies)
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .BuildOptions(new EndpointIdentity(nameof(RunTest), 0)))
                .BuildHost();

            using (host)
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await host.RunAsync(cts.Token).ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 120_000)]
        [MemberData(nameof(TransportTestData))]
        internal async Task StartStopTest(Func<IHostBuilder, IHostBuilder> useTransport, Assembly[] endpointPluginAssemblies)
        {
            var host = useTransport(Host.CreateDefaultBuilder())
                .UseEndpoint(builder => builder
                    .WithEndpointPluginAssemblies(endpointPluginAssemblies)
                    .WithDefaultCrossCuttingConcerns()
                    .WithStatistics()
                    .BuildOptions(new EndpointIdentity(nameof(StartStopTest), 0)))
                .BuildHost();

            using (host)
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(3), cts.Token).ConfigureAwait(false);
                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }

        private Func<DependencyContainerOptions, DependencyContainerOptions> ExtendTypeProvider(params Type[] additionalTypes)
        {
            return options => options.WithManualRegistration(
                Fixture.DelegateRegistration(container =>
                {
                    container
                        .RegisterDecorator<ITypeProvider, ExtendedTypeProviderDecorator>()
                        .RegisterInstance(new TypeProviderExtension(additionalTypes));
                }));
        }

        [Component(EnLifestyle.Transient)]
        private class ThrowingMessageHandler : IMessageHandler<TestCommand>
        {
            public Task Handle(TestCommand message, GenericEndpoint.Api.Abstractions.IIntegrationContext context, CancellationToken token)
            {
                throw new InvalidOperationException(message.Id.ToString(CultureInfo.InvariantCulture));
            }
        }

        [Component(EnLifestyle.Transient)]
        private class TestMessageHandler : IMessageHandler<TestCommand>, IMessageHandler<TestEvent>, IMessageHandler<TestQuery>
        {
            public Task Handle(TestCommand message, GenericEndpoint.Api.Abstractions.IIntegrationContext context, CancellationToken token)
            {
                return Task.CompletedTask;
            }

            public Task Handle(TestEvent message, GenericEndpoint.Api.Abstractions.IIntegrationContext context, CancellationToken token)
            {
                return Task.CompletedTask;
            }

            public Task Handle(TestQuery message, GenericEndpoint.Api.Abstractions.IIntegrationContext context, CancellationToken token)
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