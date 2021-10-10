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
            var useInMemoryIntegrationTransport = new Func<IHostBuilder, IHostBuilder>(hostBuilder => hostBuilder
                .UseIntegrationTransport(builder => builder
                    .WithInMemoryIntegrationTransport()
                    .WithDefaultCrossCuttingConcerns()
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

            var useInMemoryIntegrationTransport = new Func<IHostBuilder, IHostBuilder>(hostBuilder => hostBuilder
                .UseIntegrationTransport(builder => builder
                    .WithInMemoryIntegrationTransport()
                    .WithDefaultCrossCuttingConcerns()
                    .ModifyContainerOptions(options => options.WithManualRegistrations(new MessagesCollectorManualRegistration()))
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

            var useInMemoryIntegrationTransport = new Func<IHostBuilder, IHostBuilder>(hostBuilder => hostBuilder
                .UseIntegrationTransport(builder => builder
                    .WithInMemoryIntegrationTransport()
                    .WithDefaultCrossCuttingConcerns()
                    .WithTracing()
                    .ModifyContainerOptions(options => options.WithManualRegistrations(new MessagesCollectorManualRegistration()))
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
                .Resolve<IModelComparator>()
                .ExtractDiff(actualModel, expectedModel)
                .ToArray();

            modelChanges.Each((change, i) => Output.WriteLine($"[{i}] {change}"));

            if (databaseProvider.GetType() == typeof(PostgreSqlDatabaseProvider))
            {
                Assert.Equal(19, modelChanges.Length);

                AssertCreateDataBase(modelChanges, 0, "SpaceEngineersDatabase");

                AssertCreateSchema(modelChanges, 1, "spaceengineers_core_tracingendpoint");
                AssertCreateTable(modelChanges, 2, typeof(CapturedMessageDatabaseEntity));
                AssertCreateTable(modelChanges, 3, typeof(TracingEndpoint.DatabaseModel.IntegrationMessageDatabaseEntity));
                AssertCreateTable(modelChanges, 4, typeof(TracingEndpoint.DatabaseModel.IntegrationMessageHeaderDatabaseEntity));

                AssertCreateSchema(modelChanges, 5, "spaceengineers_core_genericendpoint_dataaccess");
                AssertCreateTable(modelChanges, 6, typeof(InboxMessageDatabaseEntity));
                AssertCreateTable(modelChanges, 7, typeof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageDatabaseEntity));
                AssertCreateTable(modelChanges, 8, typeof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeaderDatabaseEntity));
                AssertCreateTable(modelChanges, 9, typeof(OutboxMessageDatabaseEntity));

                AssertCreateSchema(modelChanges, 10, "spaceengineers_core_dataaccess_orm_sql");
                AssertCreateView(modelChanges, 11, nameof(DatabaseColumn));
                AssertCreateView(modelChanges, 12, nameof(DatabaseIndex));
                AssertCreateView(modelChanges, 13, nameof(DatabaseSchema));
                AssertCreateView(modelChanges, 14, nameof(DatabaseView));
                AssertCreateIndex(modelChanges, 15, "spaceengineers_core_dataaccess_orm_sql__DatabaseColumn__Column_Schema_Table__Unique");
                AssertCreateIndex(modelChanges, 16, "spaceengineers_core_dataaccess_orm_sql__DatabaseIndex__Index_Schema_Table__Unique");
                AssertCreateIndex(modelChanges, 17, "spaceengineers_core_dataaccess_orm_sql__DatabaseSchema__Name__Unique");
                AssertCreateIndex(modelChanges, 18, "spaceengineers_core_dataaccess_orm_sql__DatabaseView__Query_Schema_View__Unique");
            }
            else if (databaseProvider.GetType() == typeof(InMemoryDatabaseProvider))
            {
                Assert.Equal(10, modelChanges.Length);

                AssertCreateDataBase(modelChanges, 0, "SpaceEngineersDatabase");

                AssertCreateSchema(modelChanges, 1, "spaceengineers_core_tracingendpoint");
                AssertCreateTable(modelChanges, 2, typeof(CapturedMessageDatabaseEntity));
                AssertCreateTable(modelChanges, 3, typeof(TracingEndpoint.DatabaseModel.IntegrationMessageDatabaseEntity));
                AssertCreateTable(modelChanges, 4, typeof(TracingEndpoint.DatabaseModel.IntegrationMessageHeaderDatabaseEntity));

                AssertCreateSchema(modelChanges, 5, "spaceengineers_core_genericendpoint_dataaccess");
                AssertCreateTable(modelChanges, 6, typeof(InboxMessageDatabaseEntity));
                AssertCreateTable(modelChanges, 7, typeof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageDatabaseEntity));
                AssertCreateTable(modelChanges, 8, typeof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeaderDatabaseEntity));
                AssertCreateTable(modelChanges, 9, typeof(OutboxMessageDatabaseEntity));
            }
            else
            {
                throw new NotSupportedException(databaseProvider.GetType().FullName);
            }

            static void AssertCreateDataBase(IModelChange[] modelChanges, int index, string database)
            {
                Assert.True(modelChanges[index] is CreateDatabase);
                var createDatabase = (CreateDatabase)modelChanges[index];
                Assert.True(createDatabase.Database.Equals(database, StringComparison.OrdinalIgnoreCase));
            }

            static void AssertCreateSchema(IModelChange[] modelChanges, int index, string schema)
            {
                Assert.True(modelChanges[index] is CreateSchema);
                var createSchema = (CreateSchema)modelChanges[index];
                Assert.True(createSchema.Schema.Equals(schema, StringComparison.OrdinalIgnoreCase));
            }

            static void AssertCreateTable(IModelChange[] modelChanges, int index, Type table)
            {
                Assert.True(modelChanges[index] is CreateTable);
                var createTable = (CreateTable)modelChanges[index];
                Assert.Equal(table.Name, createTable.Table);
            }

            static void AssertCreateView(IModelChange[] modelChanges, int index, string view)
            {
                Assert.True(modelChanges[index] is CreateView);
                var createView = (CreateView)modelChanges[index];
                Assert.True(createView.View.Equals(view, StringComparison.OrdinalIgnoreCase));
            }

            static void AssertCreateIndex(IModelChange[] modelChanges, int index, string indexName)
            {
                Assert.True(modelChanges[index] is CreateIndex);
                var createIndex = (CreateIndex)modelChanges[index];
                Assert.True(createIndex.Index.Equals(indexName, StringComparison.OrdinalIgnoreCase));
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostWithDataAccessAndIntegrationTransportTracingTestData))]
        internal async Task GetConversationTraceTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
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

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();
            var collector = transportDependencyContainer.Resolve<MessagesCollector>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

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

                Assert.Empty(collector.ErrorMessages);
                Assert.Equal(4, collector.Messages.Count);
                collector.Messages.Clear();

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