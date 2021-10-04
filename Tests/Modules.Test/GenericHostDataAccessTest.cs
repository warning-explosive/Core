namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using DataAccess.Orm.Connection;
    using DataAccess.Orm.InMemoryDatabase;
    using DataAccess.Orm.Model;
    using DataAccess.Orm.PostgreSql;
    using DataAccess.Orm.Sql.Model;
    using GenericEndpoint.DataAccess.DatabaseModel;
    using GenericEndpoint.Host;
    using GenericEndpoint.Messaging.MessageHeaders;
    using GenericHost;
    using IntegrationTransport.Host;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.Hosting;
    using Mocks;
    using Registrations;
    using TracingEndpoint.Contract.Messages;
    using TracingEndpoint.DatabaseModel;
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
                // TODO: #110 - new object[] { new PostgreSqlDatabaseProvider() },
                new object[] { new InMemoryDatabaseProvider() }
            };
        }

        /// <summary>
        /// useContainer; useTransport; databaseProvider;
        /// </summary>
        /// <returns>RunHostWithDataAccessTestData</returns>
        public static IEnumerable<object[]> BuildHostWithDataAccessTestData()
        {
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
                new object[] { useInMemoryIntegrationTransport }
            };

            return DependencyContainerImplementations()
                .SelectMany(useContainer => integrationTransportProviders
                    .SelectMany(useTransport => DatabaseProviders()
                        .Select(databaseProvider => useContainer
                            .Concat(useTransport)
                            .Concat(databaseProvider)
                            .ToArray())));
        }

        /// <summary>
        /// useContainer; useTransport; collector; databaseProvider; timeout;
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
                        .Select(databaseProvider => useContainer
                            .Concat(useTransport)
                            .Concat(databaseProvider)
                            .Concat(new object[] { timeout })
                            .ToArray())));
        }

        /// <summary>
        /// useContainer; useTransport; collector; databaseProvider; timeout;
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
                        .Select(databaseProvider => useContainer
                            .Concat(useTransport)
                            .Concat(databaseProvider)
                            .Concat(new object[] { timeout })
                            .ToArray())));
        }

        [SuppressMessage("Analysis", "CA1502", Justification = "Arrange-Act-Assert")]
        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostWithDataAccessTestData))]
        internal async Task ExtractDatabaseModelChangesDiffTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            IDatabaseProvider databaseProvider)
        {
            Output.WriteLine(databaseProvider.GetType().FullName);

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseTracingEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithDataAccess(databaseProvider)
                    .BuildOptions(TestIdentity.TracingEndpoint))
                .BuildHost();

            var tracingEndpointContainer = host.GetEndpointDependencyContainer(TestIdentity.TracingEndpoint);

            var actualModel = await tracingEndpointContainer
                .Resolve<IDatabaseModelBuilder>()
                .BuildModel(CancellationToken.None)
                .ConfigureAwait(false);

            var expectedModel = await tracingEndpointContainer
                .Resolve<ICodeModelBuilder>()
                .BuildModel(CancellationToken.None)
                .ConfigureAwait(false);

            var modelChanges = tracingEndpointContainer
                .Resolve<IDatabaseModelComparator>()
                .ExtractDiff(actualModel, expectedModel)
                .ToArray();

            modelChanges.Each((change, i) => Output.WriteLine($"[{i}] {change}"));

            if (databaseProvider.GetType() == typeof(PostgreSqlDatabaseProvider))
            {
                Assert.Equal(37, modelChanges.Length);
            }
            else if (databaseProvider.GetType() == typeof(InMemoryDatabaseProvider))
            {
                Assert.Equal(33, modelChanges.Length);
            }
            else
            {
                throw new NotSupportedException(databaseProvider.GetType().FullName);
            }

            Assert.True(modelChanges[0] is CreateDatabase && ((CreateDatabase)modelChanges[0]).Name.Equals("SpaceEngineersDatabase", StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[1] is CreateSchema && ((CreateSchema)modelChanges[1]).Name.Equals("spaceengineers_core_tracingendpoint", StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[2] is CreateTable && ((CreateTable)modelChanges[2]).Table.Type == typeof(CapturedMessageDatabaseEntity));
            Assert.True(modelChanges[3] is CreateColumn && ((CreateColumn)modelChanges[3]).Column.Name.Equals(nameof(CapturedMessageDatabaseEntity.Message), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[4] is CreateColumn && ((CreateColumn)modelChanges[4]).Column.Name.Equals(nameof(CapturedMessageDatabaseEntity.RefuseReason), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[5] is CreateColumn && ((CreateColumn)modelChanges[5]).Column.Name.Equals(nameof(CapturedMessageDatabaseEntity.PrimaryKey), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[6] is CreateTable && ((CreateTable)modelChanges[6]).Table.Type == typeof(TracingEndpoint.DatabaseModel.IntegrationMessageDatabaseEntity));
            Assert.True(modelChanges[7] is CreateColumn && ((CreateColumn)modelChanges[7]).Column.Name.Equals(nameof(TracingEndpoint.DatabaseModel.IntegrationMessageDatabaseEntity.MessageId), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[8] is CreateColumn && ((CreateColumn)modelChanges[8]).Column.Name.Equals(nameof(TracingEndpoint.DatabaseModel.IntegrationMessageDatabaseEntity.ConversationId), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[9] is CreateColumn && ((CreateColumn)modelChanges[9]).Column.Name.Equals(nameof(TracingEndpoint.DatabaseModel.IntegrationMessageDatabaseEntity.Payload), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[10] is CreateColumn && ((CreateColumn)modelChanges[10]).Column.Name.Equals(nameof(TracingEndpoint.DatabaseModel.IntegrationMessageDatabaseEntity.Headers), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[11] is CreateColumn && ((CreateColumn)modelChanges[11]).Column.Name.Equals(nameof(TracingEndpoint.DatabaseModel.IntegrationMessageDatabaseEntity.PrimaryKey), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[12] is CreateTable && ((CreateTable)modelChanges[12]).Table.Type == typeof(TracingEndpoint.DatabaseModel.IntegrationMessageHeaderDatabaseEntity));
            Assert.True(modelChanges[13] is CreateColumn && ((CreateColumn)modelChanges[13]).Column.Name.Equals(nameof(TracingEndpoint.DatabaseModel.IntegrationMessageHeaderDatabaseEntity.Value), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[14] is CreateColumn && ((CreateColumn)modelChanges[14]).Column.Name.Equals(nameof(TracingEndpoint.DatabaseModel.IntegrationMessageHeaderDatabaseEntity.PrimaryKey), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[15] is CreateSchema && ((CreateSchema)modelChanges[15]).Name.Equals("spaceengineers_core_genericendpoint_dataaccess", StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[16] is CreateTable && ((CreateTable)modelChanges[16]).Table.Type == typeof(InboxMessageDatabaseEntity));
            Assert.True(modelChanges[17] is CreateColumn && ((CreateColumn)modelChanges[17]).Column.Name.Equals(nameof(InboxMessageDatabaseEntity.Message), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[18] is CreateColumn && ((CreateColumn)modelChanges[18]).Column.Name.Equals(nameof(InboxMessageDatabaseEntity.EndpointIdentity), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[19] is CreateColumn && ((CreateColumn)modelChanges[19]).Column.Name.Equals(nameof(InboxMessageDatabaseEntity.IsError), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[20] is CreateColumn && ((CreateColumn)modelChanges[20]).Column.Name.Equals(nameof(InboxMessageDatabaseEntity.Handled), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[21] is CreateColumn && ((CreateColumn)modelChanges[21]).Column.Name.Equals(nameof(InboxMessageDatabaseEntity.PrimaryKey), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[22] is CreateTable && ((CreateTable)modelChanges[22]).Table.Type == typeof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageDatabaseEntity));
            Assert.True(modelChanges[23] is CreateColumn && ((CreateColumn)modelChanges[23]).Column.Name.Equals(nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageDatabaseEntity.Payload), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[24] is CreateColumn && ((CreateColumn)modelChanges[24]).Column.Name.Equals(nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageDatabaseEntity.Headers), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[25] is CreateColumn && ((CreateColumn)modelChanges[25]).Column.Name.Equals(nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageDatabaseEntity.PrimaryKey), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[26] is CreateTable && ((CreateTable)modelChanges[26]).Table.Type == typeof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeaderDatabaseEntity));
            Assert.True(modelChanges[27] is CreateColumn && ((CreateColumn)modelChanges[27]).Column.Name.Equals(nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeaderDatabaseEntity.Value), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[28] is CreateColumn && ((CreateColumn)modelChanges[28]).Column.Name.Equals(nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeaderDatabaseEntity.PrimaryKey), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[29] is CreateTable && ((CreateTable)modelChanges[29]).Table.Type == typeof(OutboxMessageDatabaseEntity));
            Assert.True(modelChanges[30] is CreateColumn && ((CreateColumn)modelChanges[30]).Column.Name.Equals(nameof(OutboxMessageDatabaseEntity.Message), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[31] is CreateColumn && ((CreateColumn)modelChanges[31]).Column.Name.Equals(nameof(OutboxMessageDatabaseEntity.Sent), StringComparison.OrdinalIgnoreCase));
            Assert.True(modelChanges[32] is CreateColumn && ((CreateColumn)modelChanges[32]).Column.Name.Equals(nameof(OutboxMessageDatabaseEntity.PrimaryKey), StringComparison.OrdinalIgnoreCase));

            if (databaseProvider.GetType() == typeof(PostgreSqlDatabaseProvider))
            {
                Assert.True(modelChanges[33] is CreateSchema && ((CreateSchema)modelChanges[33]).Name.Equals("spaceengineers_core_dataaccess_orm_sql", StringComparison.OrdinalIgnoreCase));
                Assert.True(modelChanges[34] is CreateView && ((CreateView)modelChanges[34]).Type == typeof(DatabaseColumn));
                Assert.True(modelChanges[35] is CreateView && ((CreateView)modelChanges[35]).Type == typeof(DatabaseSchema));
                Assert.True(modelChanges[36] is CreateView && ((CreateView)modelChanges[36]).Type == typeof(DatabaseView));
            }
            else if (databaseProvider.GetType() == typeof(InMemoryDatabaseProvider))
            {
            }
            else
            {
                throw new NotSupportedException(databaseProvider.GetType().FullName);
            }
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
                    .BuildOptions(TestIdentity.TracingEndpoint))
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