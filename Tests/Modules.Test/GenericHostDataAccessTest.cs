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
    using DatabaseEntities.Mtm;
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

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostWithDataAccessTestData))]
        internal async Task CompareEquivalentDatabaseDatabaseModelsTest(
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
                .ExtractDiff(actualModel, expectedModel);

            Assert.NotEmpty(modelChanges);

            modelChanges = tracingEndpointContainer
                .Resolve<IModelComparator>()
                .ExtractDiff(expectedModel, expectedModel);

            Assert.Empty(modelChanges);
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostWithDataAccessTestData))]
        internal async Task ExtractDatabaseModelChangesDiffTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<IHostBuilder, IHostBuilder> useTransport,
            IDatabaseProvider databaseProvider)
        {
            Output.WriteLine(databaseProvider.GetType().FullName);

            var additionalOurTypes = new[]
            {
                typeof(Community),
                typeof(Participant)
            };

            var host = useTransport(Host.CreateDefaultBuilder())
                .UseContainer(useContainer)
                .UseTracingEndpoint(builder => builder
                    .WithDefaultCrossCuttingConcerns()
                    .WithDataAccess(databaseProvider)
                    .ModifyContainerOptions(options => options.WithAdditionalOurTypes(additionalOurTypes))
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
                Assert.Equal(29, modelChanges.Length);

                AssertCreateDataBase(modelChanges, 0, "SpaceEngineerDatabase");

                AssertCreateSchema(modelChanges, 1, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(Core.TracingEndpoint)));
                AssertCreateSchema(modelChanges, 2, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.DataAccess)));
                AssertCreateSchema(modelChanges, 3, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.Sql)));
                AssertCreateSchema(modelChanges, 4, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(Core.Modules), nameof(Core.Modules.Test)));

                AssertCreateTable(modelChanges, 5, typeof(TracingEndpoint.DatabaseModel.IntegrationMessage));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 5, "PrimaryKey", "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 5, nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.MessageId), "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 5, nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.ConversationId), "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 5, "Payload_SystemType_Assembly", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 5, "Payload_SystemType_Type", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 5, "Payload_Value", "not null");
                AssertMtmColumn(tracingEndpointContainer, modelChanges, 5, "Headers_Left");

                AssertCreateTable(modelChanges, 6, typeof(TracingEndpoint.DatabaseModel.IntegrationMessageHeader));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 6, "PrimaryKey", "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 6, "Value_SystemType_Assembly", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 6, "Value_SystemType_Type", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 6, "Value_Value", "not null");

                AssertCreateTable(modelChanges, 7, typeof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessage));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 7, "PrimaryKey", "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 7, "Payload_SystemType_Assembly", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 7, "Payload_SystemType_Type", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 7, "Payload_Value", "not null");
                AssertMtmColumn(tracingEndpointContainer, modelChanges, 7, "Headers_Left");

                AssertCreateTable(modelChanges, 8, typeof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeader));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 8, "PrimaryKey", "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 8, "Value_SystemType_Assembly", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 8, "Value_SystemType_Type", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 8, "Value_Value", "not null");

                AssertCreateTable(modelChanges, 9, typeof(Community));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 9, "PrimaryKey", "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 9, "Name", "not null");
                AssertMtmColumn(tracingEndpointContainer, modelChanges, 9, "Participants_Left");

                AssertCreateTable(modelChanges, 10, typeof(Participant));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 10, "PrimaryKey", "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 10, "Name", "not null");
                AssertMtmColumn(tracingEndpointContainer, modelChanges, 10, "Communities_Right");

                AssertCreateTable(modelChanges, 11, typeof(CapturedMessage));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 11, "PrimaryKey", "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 11, "Message_PrimaryKey", @"not null references ""SpaceEngineersCoreTracingEndpoint"".""IntegrationMessage"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 11, nameof(CapturedMessage.RefuseReason), string.Empty);

                AssertCreateMtmTable(modelChanges, 12, "SpaceEngineersCoreTracingEndpoint", "IntegrationMessage_IntegrationMessageHeader");
                AssertHasNoColumn(tracingEndpointContainer, modelChanges, 12, nameof(IDatabaseEntity<Guid>.PrimaryKey));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 12, "Left", @"not null references ""SpaceEngineersCoreTracingEndpoint"".""IntegrationMessage"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 12, "Right", @"not null references ""SpaceEngineersCoreTracingEndpoint"".""IntegrationMessageHeader"" (""PrimaryKey"")");

                AssertCreateTable(modelChanges, 13, typeof(InboxMessage));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 13, "PrimaryKey", "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 13, "Message_PrimaryKey", @"not null references ""SpaceEngineersCoreGenericEndpointDataAccess"".""IntegrationMessage"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 13, "EndpointIdentity_LogicalName", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 13, "EndpointIdentity_InstanceName", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 13, nameof(InboxMessage.IsError), "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 13, nameof(InboxMessage.Handled), "not null");

                AssertCreateMtmTable(modelChanges, 14, "SpaceEngineersCoreGenericEndpointDataAccess", "IntegrationMessage_IntegrationMessageHeader");
                AssertHasNoColumn(tracingEndpointContainer, modelChanges, 14, nameof(IDatabaseEntity<Guid>.PrimaryKey));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 14, "Left", @"not null references ""SpaceEngineersCoreGenericEndpointDataAccess"".""IntegrationMessage"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 14, "Right", @"not null references ""SpaceEngineersCoreGenericEndpointDataAccess"".""IntegrationMessageHeader"" (""PrimaryKey"")");

                AssertCreateTable(modelChanges, 15, typeof(OutboxMessage));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 15, "PrimaryKey", "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 15, "Message_PrimaryKey", @"not null references ""SpaceEngineersCoreGenericEndpointDataAccess"".""IntegrationMessage"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 15, nameof(OutboxMessage.Sent), "not null");

                AssertCreateMtmTable(modelChanges, 16, "SpaceEngineersCoreModulesTest", "Community_Participant");
                AssertHasNoColumn(tracingEndpointContainer, modelChanges, 16, nameof(IDatabaseEntity<Guid>.PrimaryKey));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 16, "Left", @"not null references ""SpaceEngineersCoreModulesTest"".""Community"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 16, "Right", @"not null references ""SpaceEngineersCoreModulesTest"".""Participant"" (""PrimaryKey"")");

                AssertCreateView(modelChanges, 17, nameof(DatabaseColumn));
                AssertCreateView(modelChanges, 18, nameof(DatabaseColumnConstraint));
                AssertCreateView(modelChanges, 19, nameof(DatabaseIndex));
                AssertCreateView(modelChanges, 20, nameof(DatabaseSchema));
                AssertCreateView(modelChanges, 21, nameof(DatabaseView));

                AssertCreateIndex(modelChanges, 22, "SpaceEngineersCoreTracingEndpoint", "Left_Right");
                AssertCreateIndex(modelChanges, 23, "SpaceEngineersCoreGenericEndpointDataAccess", "Left_Right");
                AssertCreateIndex(modelChanges, 24, "SpaceEngineersCoreDataAccessOrmSql", "Column_Schema_Table");
                AssertCreateIndex(modelChanges, 25, "SpaceEngineersCoreDataAccessOrmSql", "Index_Schema_Table");
                AssertCreateIndex(modelChanges, 26, "SpaceEngineersCoreDataAccessOrmSql", "Name");
                AssertCreateIndex(modelChanges, 27, "SpaceEngineersCoreDataAccessOrmSql", "Query_Schema_View");
                AssertCreateIndex(modelChanges, 28, "SpaceEngineersCoreModulesTest", "Left_Right");

                static void AssertCreateTable(IModelChange[] modelChanges, int index, Type table)
                {
                    Assert.True(modelChanges[index] is CreateTable);
                    var createTable = (CreateTable)modelChanges[index];
                    Assert.Equal(table.Name, createTable.Table);
                }

                static void AssertCreateMtmTable(IModelChange[] modelChanges, int index, string schema, string table)
                {
                    Assert.True(modelChanges[index] is CreateTable);
                    var createTable = (CreateTable)modelChanges[index];
                    Assert.Equal(schema, createTable.Schema);
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
                Assert.Equal(13, modelChanges.Length);

                AssertCreateDataBase(modelChanges, 0, "SpaceEngineerDatabase");

                AssertCreateSchema(modelChanges, 1, AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.TracingEndpoint)));
                AssertCreateTable(modelChanges, 2, typeof(CapturedMessage));
                AssertCreateTable(modelChanges, 3, typeof(TracingEndpoint.DatabaseModel.IntegrationMessage));
                AssertCreateTable(modelChanges, 4, typeof(TracingEndpoint.DatabaseModel.IntegrationMessageHeader));

                AssertCreateSchema(modelChanges, 5, AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.DataAccess)));
                AssertCreateTable(modelChanges, 6, typeof(InboxMessage));
                AssertCreateTable(modelChanges, 7, typeof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessage));
                AssertCreateTable(modelChanges, 8, typeof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeader));
                AssertCreateTable(modelChanges, 9, typeof(OutboxMessage));

                AssertCreateSchema(modelChanges, 10, AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.Modules), nameof(Core.Modules.Test)));
                AssertCreateTable(modelChanges, 11, typeof(Community));
                AssertCreateTable(modelChanges, 12, typeof(Participant));

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

            static void AssertCreateIndex(IModelChange[] modelChanges, int index, string schema, string indexName)
            {
                Assert.True(modelChanges[index] is CreateIndex);
                var createIndex = (CreateIndex)modelChanges[index];
                Assert.True(createIndex.Schema.Equals(schema, StringComparison.OrdinalIgnoreCase));
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