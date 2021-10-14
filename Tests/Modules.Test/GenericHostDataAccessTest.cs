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
    using DataAccess.Api.Model;
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
                new object[] { new PostgreSqlDatabaseProvider() },
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

            var unorderedModelChanges = tracingEndpointContainer
                .Resolve<IModelComparator>()
                .ExtractDiff(actualModel, expectedModel);

            var modelChanges = tracingEndpointContainer
                .Resolve<IModelChangesSorter>()
                .Sort(unorderedModelChanges)
                .ToArray();

            modelChanges.Each((change, i) => Output.WriteLine($"[{i}] {change}"));

            if (databaseProvider.GetType() == typeof(PostgreSqlDatabaseProvider))
            {
                Assert.Equal(25, modelChanges.Length);

                AssertCreateDataBase(modelChanges, 0, "SpaceEngineerDatabase");

                AssertCreateSchema(modelChanges, 1, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(Core.TracingEndpoint)));
                AssertCreateSchema(modelChanges, 2, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.DataAccess)));
                AssertCreateSchema(modelChanges, 3, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.Sql)));
                AssertCreateSchema(modelChanges, 4, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(Core.Basics), nameof(Core.Dynamic)));

                AssertCreateTable(modelChanges, 5, typeof(TracingEndpoint.DatabaseModel.IntegrationMessageDatabaseEntity));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 5, "PrimaryKey", "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 5, nameof(TracingEndpoint.DatabaseModel.IntegrationMessageDatabaseEntity.MessageId), "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 5, nameof(TracingEndpoint.DatabaseModel.IntegrationMessageDatabaseEntity.ConversationId), "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 5, "Payload_SystemType_Assembly", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 5, "Payload_SystemType_Type", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 5, "Payload_Value", "not null");
                AssertMtmColumn(tracingEndpointContainer, modelChanges, 5, "Headers_Left");

                AssertCreateTable(modelChanges, 6, typeof(TracingEndpoint.DatabaseModel.IntegrationMessageHeaderDatabaseEntity));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 6, "PrimaryKey", "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 6, "Value_SystemType_Assembly", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 6, "Value_SystemType_Type", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 6, "Value_Value", "not null");

                AssertCreateTable(modelChanges, 7, typeof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageDatabaseEntity));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 7, "PrimaryKey", "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 7, "Payload_SystemType_Assembly", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 7, "Payload_SystemType_Type", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 7, "Payload_Value", "not null");
                AssertMtmColumn(tracingEndpointContainer, modelChanges, 7, "Headers_Left");

                AssertCreateTable(modelChanges, 8, typeof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeaderDatabaseEntity));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 8, "PrimaryKey", "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 8, "Value_SystemType_Assembly", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 8, "Value_SystemType_Type", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 8, "Value_Value", "not null");

                AssertCreateTable(modelChanges, 9, typeof(CapturedMessageDatabaseEntity));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 9, "PrimaryKey", "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 9, "Message_PrimaryKey", @"not null references ""SpaceEngineersCoreTracingEndpoint"".""IntegrationMessageDatabaseEntity"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 9, nameof(CapturedMessageDatabaseEntity.RefuseReason), string.Empty);

                AssertCreateDynamicTable(modelChanges, 10, "SpaceEngineersCoreTracingEndpoint_IntegrationMessageDatabaseEntity_Headers_SpaceEngineersCoreTracingEndpoint_IntegrationMessageHeaderDatabaseEntity");
                AssertHasNoColumn(tracingEndpointContainer, modelChanges, 10, nameof(IDatabaseEntity<Guid>.PrimaryKey));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 10, "Left", @"not null references ""SpaceEngineersCoreTracingEndpoint"".""IntegrationMessageDatabaseEntity"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 10, "Right", @"not null references ""SpaceEngineersCoreTracingEndpoint"".""IntegrationMessageHeaderDatabaseEntity"" (""PrimaryKey"")");

                AssertCreateDynamicTable(modelChanges, 11, "SpaceEngineersCoreGenericEndpointDataAccess_IntegrationMessageDatabaseEntity_Headers_SpaceEngineersCoreGenericEndpointDataAccess_IntegrationMessageHeaderDatabaseEntity");
                AssertHasNoColumn(tracingEndpointContainer, modelChanges, 11, nameof(IDatabaseEntity<Guid>.PrimaryKey));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 11, "Left", @"not null references ""SpaceEngineersCoreGenericEndpointDataAccess"".""IntegrationMessageDatabaseEntity"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 11, "Right", @"not null references ""SpaceEngineersCoreGenericEndpointDataAccess"".""IntegrationMessageHeaderDatabaseEntity"" (""PrimaryKey"")");

                AssertCreateTable(modelChanges, 12, typeof(InboxMessageDatabaseEntity));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 12, "PrimaryKey", "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 12, "Message_PrimaryKey", @"not null references ""SpaceEngineersCoreGenericEndpointDataAccess"".""IntegrationMessageDatabaseEntity"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 12, "EndpointIdentity_LogicalName", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 12, "EndpointIdentity_InstanceName", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 12, nameof(InboxMessageDatabaseEntity.IsError), "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 12, nameof(InboxMessageDatabaseEntity.Handled), "not null");

                AssertCreateTable(modelChanges, 13, typeof(OutboxMessageDatabaseEntity));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 13, "PrimaryKey", "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 13, "Message_PrimaryKey", @"not null references ""SpaceEngineersCoreGenericEndpointDataAccess"".""IntegrationMessageDatabaseEntity"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 13, nameof(OutboxMessageDatabaseEntity.Sent), "not null");

                AssertCreateView(modelChanges, 14, nameof(DatabaseColumn));
                AssertCreateView(modelChanges, 15, nameof(DatabaseColumnConstraint));
                AssertCreateView(modelChanges, 16, nameof(DatabaseIndex));
                AssertCreateView(modelChanges, 17, nameof(DatabaseSchema));
                AssertCreateView(modelChanges, 18, nameof(DatabaseView));

                AssertCreateIndex(modelChanges, 19, "SpaceEngineersCoreBasicsDynamic__SpaceEngineersCoreTracingEndpoint_IntegrationMessageDatabaseEntity_Headers_SpaceEngineersCoreTracingEndpoint_IntegrationMessageHeaderDatabaseEntity__Left_Right__Unique");
                AssertCreateIndex(modelChanges, 20, "SpaceEngineersCoreBasicsDynamic__SpaceEngineersCoreGenericEndpointDataAccess_IntegrationMessageDatabaseEntity_Headers_SpaceEngineersCoreGenericEndpointDataAccess_IntegrationMessageHeaderDatabaseEntity__Left_Right__Unique");
                AssertCreateIndex(modelChanges, 21, "SpaceEngineersCoreDataAccessOrmSql__DatabaseColumn__Column_Schema_Table__Unique");
                AssertCreateIndex(modelChanges, 22, "SpaceEngineersCoreDataAccessOrmSql__DatabaseIndex__Index_Schema_Table__Unique");
                AssertCreateIndex(modelChanges, 23, "SpaceEngineersCoreDataAccessOrmSql__DatabaseSchema__Name__Unique");
                AssertCreateIndex(modelChanges, 24, "SpaceEngineersCoreDataAccessOrmSql__DatabaseView__Query_Schema_View__Unique");

                static void AssertCreateTable(IModelChange[] modelChanges, int index, Type table)
                {
                    Assert.True(modelChanges[index] is CreateTable);
                    var createTable = (CreateTable)modelChanges[index];
                    Assert.Equal(table.Name, createTable.Table);
                }

                static void AssertCreateDynamicTable(IModelChange[] modelChanges, int index, string table)
                {
                    Assert.True(modelChanges[index] is CreateTable);
                    var createTable = (CreateTable)modelChanges[index];
                    Assert.Equal(table, createTable.Table);
                }

                static void AssertColumnConstraints(IDependencyContainer dependencyContainer, IModelChange[] modelChanges, int index, string column, string constraints)
                {
                    Assert.True(modelChanges[index] is CreateTable);
                    var createTable = (CreateTable)modelChanges[index];
                    var modelProvider = dependencyContainer.Resolve<IModelProvider>();
                    Assert.True(modelProvider.Model.ContainsKey(createTable.Schema));
                    Assert.True(modelProvider.Model[createTable.Schema].ContainsKey(createTable.Table));
                    Assert.True(modelProvider.Model[createTable.Schema][createTable.Table] is TableInfo);
                    var tableInfo = (TableInfo)modelProvider.Model[createTable.Schema][createTable.Table];
                    Assert.True(tableInfo.Columns.ContainsKey(column));
                    var columnInfo = tableInfo.Columns[column];
                    var actualConstraints = columnInfo.Constraints.ToString(" ");
                    Assert.True(actualConstraints.Equals(constraints, StringComparison.OrdinalIgnoreCase));
                    Assert.False(columnInfo.IsMultipleRelation);

                    if (constraints.Contains("references", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.NotNull(columnInfo.Relation);
                    }
                    else
                    {
                        Assert.Null(columnInfo.Relation);
                    }
                }

                static void AssertMtmColumn(IDependencyContainer dependencyContainer, IModelChange[] modelChanges, int index, string column)
                {
                    Assert.True(modelChanges[index] is CreateTable);
                    var createTable = (CreateTable)modelChanges[index];
                    var modelProvider = dependencyContainer.Resolve<IModelProvider>();
                    Assert.True(modelProvider.Model.ContainsKey(createTable.Schema));
                    Assert.True(modelProvider.Model[createTable.Schema].ContainsKey(createTable.Table));
                    Assert.True(modelProvider.Model[createTable.Schema][createTable.Table] is TableInfo);
                    var tableInfo = (TableInfo)modelProvider.Model[createTable.Schema][createTable.Table];
                    Assert.True(tableInfo.Columns.ContainsKey(column));
                    var columnInfo = tableInfo.Columns[column];
                    Assert.True(columnInfo.IsMultipleRelation);
                    Assert.NotNull(columnInfo.Relation);
                }

                static void AssertHasNoColumn(IDependencyContainer dependencyContainer, IModelChange[] modelChanges, int index, string column)
                {
                    Assert.True(modelChanges[index] is CreateTable);
                    var createTable = (CreateTable)modelChanges[index];
                    var modelProvider = dependencyContainer.Resolve<IModelProvider>();
                    Assert.True(modelProvider.Model.ContainsKey(createTable.Schema));
                    Assert.True(modelProvider.Model[createTable.Schema].ContainsKey(createTable.Table));
                    Assert.True(modelProvider.Model[createTable.Schema][createTable.Table] is TableInfo);
                    var tableInfo = (TableInfo)modelProvider.Model[createTable.Schema][createTable.Table];
                    Assert.True(tableInfo.Columns.Keys.All(key => !key.Contains(column, StringComparison.OrdinalIgnoreCase)));
                }
            }
            else if (databaseProvider.GetType() == typeof(InMemoryDatabaseProvider))
            {
                Assert.Equal(10, modelChanges.Length);

                AssertCreateDataBase(modelChanges, 0, "SpaceEngineerDatabase");

                AssertCreateSchema(modelChanges, 1, AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.TracingEndpoint)));
                AssertCreateTable(modelChanges, 2, typeof(CapturedMessageDatabaseEntity));
                AssertCreateTable(modelChanges, 3, typeof(TracingEndpoint.DatabaseModel.IntegrationMessageDatabaseEntity));
                AssertCreateTable(modelChanges, 4, typeof(TracingEndpoint.DatabaseModel.IntegrationMessageHeaderDatabaseEntity));

                AssertCreateSchema(modelChanges, 5, AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.DataAccess)));
                AssertCreateTable(modelChanges, 6, typeof(InboxMessageDatabaseEntity));
                AssertCreateTable(modelChanges, 7, typeof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageDatabaseEntity));
                AssertCreateTable(modelChanges, 8, typeof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeaderDatabaseEntity));
                AssertCreateTable(modelChanges, 9, typeof(OutboxMessageDatabaseEntity));

                static void AssertCreateTable(IModelChange[] modelChanges, int index, Type table)
                {
                    Assert.True(modelChanges[index] is CreateTable);
                    var createTable = (CreateTable)modelChanges[index];
                    Assert.Equal(table.FullName, createTable.Table);
                }
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

            if (databaseProvider.GetType() == typeof(PostgreSqlDatabaseProvider))
            {
                throw new NotImplementedException("#110");
            }

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