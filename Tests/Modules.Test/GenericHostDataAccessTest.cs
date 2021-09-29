namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using DataAccess.Orm.Connection;
    using DataAccess.Orm.InMemoryDatabase;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Host;
    using GenericEndpoint.Messaging.MessageHeaders;
    using GenericHost;
    using IntegrationTransport.Host;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.Hosting;
    using Mocks;
    using Registrations;
    using TracingEndpoint.Contract;
    using TracingEndpoint.Contract.Messages;
    using TracingEndpoint.Host;
    using Xunit;
    using Xunit.Abstractions;
    using IIntegrationContext = IntegrationTransport.Api.Abstractions.IIntegrationContext;

    /// <summary>
    /// GenericHost assembly tests
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "reviewed")]
    public class GenericHostDataAccessTest : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public GenericHostDataAccessTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// useContainer
        /// </summary>
        /// <returns>DependencyContainerImplementations</returns>
        public static IEnumerable<object[]> DependencyContainerImplementations()
        {
            var dependencyContainerProducers = new object[]
            {
                new Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>>(options => options.UseGenericContainer())
            };

            return new[] { dependencyContainerProducers };
        }

        /// <summary>
        /// databaseProvider
        /// </summary>
        /// <returns>DatabaseProviders</returns>
        public static IEnumerable<object[]> DatabaseProviders()
        {
            return new[]
            {
                // TODO: #135 - new object[] { new PostgreSqlDatabaseProvider() },
                new object[] { new InMemoryDatabaseProvider() }
            };
        }

        /// <summary>
        /// useContainer; useTransport; databaseProvider; timeout;
        /// </summary>
        /// <returns>RunHostWithDataAccessTestData</returns>
        public static IEnumerable<object[]> RunHostWithDataAccessTestData()
        {
            var timeout = TimeSpan.FromSeconds(300);

            var inMemoryIntegrationTransportCollector = new MessagesCollector();
            var useInMemoryIntegrationTransport = new Func<IHostBuilder, IHostBuilder>(hostBuilder => hostBuilder
                .UseIntegrationTransport(builder => builder
                    .WithInMemoryIntegrationTransport()
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options
                        .WithManualRegistrations(new MessagesCollectorInstanceManualRegistration(inMemoryIntegrationTransportCollector)))
                    .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                new object[] { useInMemoryIntegrationTransport, inMemoryIntegrationTransportCollector }
            };

            return DependencyContainerImplementations()
                .SelectMany(useContainer => integrationTransportProviders
                    .SelectMany(useTransport => DatabaseProviders()
                        .Select(databaseProvider => useContainer.Concat(useTransport).Concat(databaseProvider).Concat(new object[] { timeout }).ToArray())));
        }

        /// <summary>
        /// useContainer; useTransport; databaseProvider; timeout;
        /// </summary>
        /// <returns>RunHostWithDataAccessTestData</returns>
        public static IEnumerable<object[]> RunHostWithDataAccessAndIntegrationTransportTracingTestData()
        {
            var timeout = TimeSpan.FromSeconds(300);

            var inMemoryIntegrationTransportCollector = new MessagesCollector();
            var useInMemoryIntegrationTransport = new Func<IHostBuilder, IHostBuilder>(hostBuilder => hostBuilder
                .UseIntegrationTransport(builder => builder
                    .WithInMemoryIntegrationTransport()
                    .WithDefaultCrossCuttingConcerns()
                    .WithTracing()
                    .ModifyContainerOptions(options => options
                        .WithManualRegistrations(new MessagesCollectorInstanceManualRegistration(inMemoryIntegrationTransportCollector)))
                    .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                new object[] { useInMemoryIntegrationTransport, inMemoryIntegrationTransportCollector }
            };

            return DependencyContainerImplementations()
                .SelectMany(useContainer => integrationTransportProviders
                    .SelectMany(useTransport => DatabaseProviders()
                        .Select(databaseProvider => useContainer.Concat(useTransport).Concat(databaseProvider).Concat(new object[] { timeout }).ToArray())));
        }

        [Fact(Timeout = 60_000)]
        /*[MemberData(nameof(RunHostWithDataAccessTestData))]*/
        internal void BuildDatabaseModelTest()
        {
            // TODO: #110 - Model builder & migrations
            /*var tracingEndpointIdentity = new EndpointIdentity(TracingEndpointIdentity.LogicalName, 0);

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseTracingEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithDataAccess(databaseProvider)
                    .BuildOptions(tracingEndpointIdentity))
                .BuildHost();

            var tracingEndpointContainer = host.GetEndpointDependencyContainer(tracingEndpointIdentity);

            var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var actualModel = await tracingEndpointContainer
                    .Resolve<IDatabaseModelBuilder>()
                    .BuildModel(cts.Token)
                    .ConfigureAwait(false);

                var expectedModel = await tracingEndpointContainer
                    .Resolve<ICodeModelBuilder>()
                    .BuildModel(cts.Token)
                    .ConfigureAwait(false);

                var modelChanges = tracingEndpointContainer
                    .Resolve<IDatabaseModelComparator>()
                    .ExtractDiff(actualModel, expectedModel)
                    .ToList();

                modelChanges.Each(change => Output.WriteLine(change.ToString()));
                Assert.NotEmpty(modelChanges);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }*/
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostWithDataAccessAndIntegrationTransportTracingTestData))]
        internal async Task GetConversationTraceTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            MessagesCollector collector,
            IDatabaseProvider databaseProvider,
            TimeSpan timeout)
        {
            Output.WriteLine(databaseProvider.GetType().FullName);

            var messageTypes = new[]
            {
                typeof(Query),
                typeof(Reply)
            };

            var messageHandlerTypes = new[]
            {
                typeof(QueryAlwaysReplyMessageHandler)
            };

            var additionalOurTypes = messageTypes.Concat(messageHandlerTypes).ToArray();

            var tracingEndpointIdentity = new EndpointIdentity(TracingEndpointIdentity.LogicalName, 0);

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithTracing()
                    .ModifyContainerOptions(options => options.WithAdditionalOurTypes(additionalOurTypes))
                    .BuildOptions(TestIdentity.Endpoint10))
                .UseTracingEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithDataAccess(databaseProvider)
                    .BuildOptions(tracingEndpointIdentity))
                .BuildHost();

            var container = host.GetTransportDependencyContainer();

            var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var integrationContext = container.Resolve<IIntegrationContext>();

                var conversationId = Guid.NewGuid();

                var awaiter = Task.WhenAll(
                    collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.IntegrationMessage.ReflectedType == typeof(GetConversationTrace)),
                    collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.IntegrationMessage.ReflectedType == typeof(ConversationTrace)));

                var trace = await integrationContext
                    .RpcRequest<GetConversationTrace, ConversationTrace>(new GetConversationTrace(conversationId), cts.Token)
                    .ConfigureAwait(false);

                await awaiter.ConfigureAwait(false);

                Assert.Empty(collector.ErrorMessages);
                Assert.Equal(4, collector.Messages.Count);
                var messages = collector.Messages.ToArray();
                collector.Messages.Clear();
                Assert.Equal(typeof(GetConversationTrace), messages[0].ReflectedType);
                Assert.Equal(typeof(ConversationTrace), messages[1].ReflectedType);
                Assert.Equal(typeof(CaptureTrace), messages[2].ReflectedType);
                Assert.Equal(typeof(GetConversationTrace), ((CaptureTrace)messages[2].Payload).IntegrationMessage.ReflectedType);
                Assert.Equal(typeof(CaptureTrace), messages[3].ReflectedType);
                Assert.Equal(typeof(ConversationTrace), ((CaptureTrace)messages[3].Payload).IntegrationMessage.ReflectedType);

                Assert.Equal(conversationId, trace.ConversationId);
                Assert.Null(trace.Message);
                Assert.Null(trace.RefuseReason);
                Assert.Null(trace.SubsequentTrace);

                var query = new Query(42);

                awaiter = Task.WhenAll(
                    collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.IntegrationMessage.ReflectedType == typeof(Query)),
                    collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.IntegrationMessage.ReflectedType == typeof(Reply)));

                var reply = await integrationContext
                    .RpcRequest<Query, Reply>(query, cts.Token)
                    .ConfigureAwait(false);

                await awaiter.ConfigureAwait(false);

                Assert.Empty(collector.ErrorMessages);
                Assert.Equal(4, collector.Messages.Count);
                messages = collector.Messages.ToArray();
                collector.Messages.Clear();
                Assert.Equal(typeof(Query), messages[0].ReflectedType);
                Assert.Equal(typeof(Reply), messages[1].ReflectedType);
                Assert.Equal(typeof(CaptureTrace), messages[2].ReflectedType);
                Assert.Equal(typeof(Query), ((CaptureTrace)messages[2].Payload).IntegrationMessage.ReflectedType);
                Assert.Equal(typeof(CaptureTrace), messages[3].ReflectedType);
                Assert.Equal(typeof(Reply), ((CaptureTrace)messages[3].Payload).IntegrationMessage.ReflectedType);

                conversationId = messages[0].ReadRequiredHeader<ConversationId>().Value;

                awaiter = Task.WhenAll(
                    collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.IntegrationMessage.ReflectedType == typeof(GetConversationTrace)),
                    collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.IntegrationMessage.ReflectedType == typeof(ConversationTrace)));

                trace = await integrationContext
                    .RpcRequest<GetConversationTrace, ConversationTrace>(new GetConversationTrace(conversationId), cts.Token)
                    .ConfigureAwait(false);

                await awaiter.ConfigureAwait(false);

                Assert.Equal(conversationId, trace.ConversationId);
                Assert.NotNull(trace.Message);
                Assert.Equal(typeof(Query), trace.Message.ReflectedType);
                Assert.Equal(query.Id, ((Query)trace.Message.Payload).Id);
                Assert.Null(trace.RefuseReason);
                Assert.NotNull(trace.SubsequentTrace);
                Assert.Single(trace.SubsequentTrace);

                var subsequentTrace = trace.SubsequentTrace!.Single();
                Assert.NotNull(subsequentTrace.Message);
                Assert.Equal(typeof(Reply), subsequentTrace.Message.ReflectedType);
                Assert.Equal(reply.Id, ((Reply)subsequentTrace.Message.Payload).Id);
                Assert.Null(subsequentTrace.RefuseReason);
                Assert.NotNull(subsequentTrace.SubsequentTrace);
                Assert.Empty(subsequentTrace.SubsequentTrace);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }
    }
}