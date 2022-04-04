namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using CompositionRoot.Api.Abstractions.Registration;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using CrossCuttingConcerns.Api.Abstractions;
    using DataAccess.Api.Model;
    using DataAccess.Orm.Connection;
    using DataAccess.Orm.Host;
    using DataAccess.Orm.Host.Model;
    using DataAccess.Orm.PostgreSql.Host;
    using DataAccess.Orm.Sql.Host.Model;
    using DataAccess.Orm.Sql.Model;
    using DatabaseEntities.Relations;
    using GenericEndpoint.DataAccess.DatabaseModel;
    using GenericEndpoint.Host;
    using GenericEndpoint.Messaging.MessageHeaders;
    using GenericHost;
    using IntegrationTransport.Host;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.Hosting;
    using Migrations;
    using Mocks;
    using Overrides;
    using Registrations;
    using TracingEndpoint.Contract;
    using TracingEndpoint.Contract.Messages;
    using TracingEndpoint.DatabaseModel;
    using TracingEndpoint.Host;
    using Xunit;
    using Xunit.Abstractions;
    using EndpointIdentity = GenericEndpoint.Contract.EndpointIdentity;
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
        /// databaseProvider
        /// </summary>
        /// <returns>DatabaseProviders</returns>
        public static IEnumerable<IDatabaseProvider> DatabaseProviders()
        {
            return new IDatabaseProvider[]
            {
                new PostgreSqlDatabaseProvider()
            };
        }

        /// <summary>
        /// useContainer; useTransport; databaseProvider;
        /// </summary>
        /// <returns>RunHostWithDataAccessTestData</returns>
        public static IEnumerable<object[]> BuildHostWithDataAccessTestData()
        {
            var useInMemoryIntegrationTransport = new Func<ITestOutputHelper, IHostBuilder, IHostBuilder>(
                (output, hostBuilder) => hostBuilder
                   .UseIntegrationTransport(builder => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .ModifyContainerOptions(options => options
                           .WithOverrides(new TestLoggerOverride(output))
                           .WithOverrides(new TestSettingsScopeProviderOverride(nameof(ExtractDatabaseModelChangesDiffTest))))
                       .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                useInMemoryIntegrationTransport
            };

            return integrationTransportProviders
               .SelectMany(useTransport => DatabaseProviders()
                   .Select(databaseProvider => new object[]
                   {
                       useTransport,
                       databaseProvider
                   }));
        }

        /// <summary>
        /// useContainer; useTransport; collector; databaseProvider; timeout;
        /// </summary>
        /// <returns>RunHostWithDataAccessTestData</returns>
        public static IEnumerable<object[]> RunHostWithDataAccessTestData()
        {
            var timeout = TimeSpan.FromSeconds(60);

            var useInMemoryIntegrationTransport = new Func<ITestOutputHelper, IHostBuilder, IHostBuilder>(
                (output, hostBuilder) => hostBuilder
                   .UseIntegrationTransport(builder => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .ModifyContainerOptions(options => options
                           .WithManualRegistrations(new MessagesCollectorManualRegistration())
                           .WithOverrides(new TestLoggerOverride(output))
                           .WithOverrides(new TestSettingsScopeProviderOverride(nameof(ExtractDatabaseModelChangesDiffTest))))
                       .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                useInMemoryIntegrationTransport
            };

            return integrationTransportProviders
               .SelectMany(useTransport => DatabaseProviders()
                   .Select(databaseProvider => new object[]
                   {
                       useTransport,
                       databaseProvider,
                       timeout
                   }));
        }

        /// <summary>
        /// useContainer; useTransport; collector; databaseProvider; timeout;
        /// </summary>
        /// <returns>RunHostWithDataAccessTestData</returns>
        public static IEnumerable<object[]> RunHostWithDataAccessAndIntegrationTransportTracingTestData()
        {
            var timeout = TimeSpan.FromSeconds(60);

            var useInMemoryIntegrationTransport = new Func<ITestOutputHelper, IHostBuilder, IHostBuilder>(
                (output, hostBuilder) => hostBuilder
                   .UseIntegrationTransport(builder => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .WithTracing()
                       .ModifyContainerOptions(options => options
                           .WithManualRegistrations(new MessagesCollectorManualRegistration())
                           .WithOverrides(new TestLoggerOverride(output))
                           .WithOverrides(new TestSettingsScopeProviderOverride(nameof(ExtractDatabaseModelChangesDiffTest))))
                       .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                useInMemoryIntegrationTransport
            };

            return integrationTransportProviders
               .SelectMany(useTransport => DatabaseProviders()
                   .Select(databaseProvider => new object[]
                   {
                       useTransport,
                       databaseProvider,
                       timeout
                   }));
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostWithDataAccessTestData))]
        internal async Task CompareEquivalentDatabaseDatabaseModelsTest(
            Func<ITestOutputHelper, IHostBuilder, IHostBuilder> useTransport,
            IDatabaseProvider databaseProvider)
        {
            Output.WriteLine(databaseProvider.GetType().FullName);

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(Output),
                new TestSettingsScopeProviderOverride(nameof(CompareEquivalentDatabaseDatabaseModelsTest))
            };

            var host = useTransport(Output, Host.CreateDefaultBuilder())
               .UseTracingEndpoint(TestIdentity.Instance0,
                    builder => builder
                       .WithDataAccess(databaseProvider)
                       .ModifyContainerOptions(options => options
                           .WithOverrides(overrides))
                       .BuildOptions())
               .ExecuteMigrations(builder => builder
                   .WithDataAccess(databaseProvider)
                   .ModifyContainerOptions(options => options
                       .WithOverrides(overrides))
                   .BuildOptions())
               .BuildHost();

            var migrationsContainer = host.GetMigrationsDependencyContainer();

            var actualModel = await migrationsContainer
                .Resolve<IDatabaseModelBuilder>()
                .BuildModel(CancellationToken.None)
                .ConfigureAwait(false);

            var databaseEntities = migrationsContainer
               .Resolve<IDatabaseTypeProvider>()
               .DatabaseEntities()
               .ToArray();

            var expectedModel = await migrationsContainer
                .Resolve<ICodeModelBuilder>()
                .BuildModel(databaseEntities, CancellationToken.None)
                .ConfigureAwait(false);

            var modelChanges = migrationsContainer
                .Resolve<IModelComparator>()
                .ExtractDiff(actualModel, expectedModel);

            Assert.NotEmpty(modelChanges);

            modelChanges = migrationsContainer
                .Resolve<IModelComparator>()
                .ExtractDiff(expectedModel, expectedModel);

            Assert.Empty(modelChanges);
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostWithDataAccessTestData))]
        internal async Task ExtractDatabaseModelChangesDiffTest(
            Func<ITestOutputHelper, IHostBuilder, IHostBuilder> useTransport,
            IDatabaseProvider databaseProvider)
        {
            Output.WriteLine(databaseProvider.GetType().FullName);

            var additionalOurTypes = new[]
            {
                typeof(Community),
                typeof(Participant),
                typeof(Blog),
                typeof(Post),
                typeof(User)
            };

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(Output),
                new TestSettingsScopeProviderOverride(nameof(ExtractDatabaseModelChangesDiffTest))
            };

            var host = useTransport(Output, Host.CreateDefaultBuilder())
               .UseTracingEndpoint(TestIdentity.Instance0,
                    builder => builder
                       .WithDataAccess(databaseProvider)
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithOverrides(overrides))
                       .BuildOptions())
               .ExecuteMigrations(builder => builder
                   .WithDataAccess(databaseProvider)
                   .ModifyContainerOptions(options => options
                       .WithOverrides(overrides))
                   .BuildOptions())
               .BuildHost();

            var tracingEndpointContainer = host.GetEndpointDependencyContainer(
                new EndpointIdentity(TracingEndpointIdentity.LogicalName, TestIdentity.Instance0));

            var migrationsContainer = host.GetMigrationsDependencyContainer();

            var actualModel = await migrationsContainer
                .Resolve<IDatabaseModelBuilder>()
                .BuildModel(CancellationToken.None)
                .ConfigureAwait(false);

            var databaseEntities = migrationsContainer
               .Resolve<IDatabaseTypeProvider>()
               .DatabaseEntities()
               .ToArray();

            var expectedModel = await migrationsContainer
                .Resolve<ICodeModelBuilder>()
                .BuildModel(databaseEntities, CancellationToken.None)
                .ConfigureAwait(false);

            var unorderedModelChanges = migrationsContainer
                .Resolve<IModelComparator>()
                .ExtractDiff(actualModel, expectedModel);

            var modelChanges = migrationsContainer
                .Resolve<IModelChangesSorter>()
                .Sort(unorderedModelChanges)
                .ToArray();

            modelChanges.Each((change, i) => Output.WriteLine($"[{i}] {change}"));

            if (databaseProvider.GetType() == typeof(PostgreSqlDatabaseProvider))
            {
                Assert.Equal(33, modelChanges.Length);

                AssertCreateDataBase(modelChanges, 0, nameof(ExtractDatabaseModelChangesDiffTest));

                AssertCreateSchema(modelChanges, 1, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.Sql), nameof(Core.DataAccess.Orm.Sql.Host)));
                AssertCreateSchema(modelChanges, 2, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.DataAccess)));
                AssertCreateSchema(modelChanges, 3, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(Core.Modules), nameof(Core.Modules.Test)));
                AssertCreateSchema(modelChanges, 4, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(Core.TracingEndpoint)));

                AssertCreateTable(modelChanges, 5, typeof(AppliedMigration));
                AssertColumnConstraints(migrationsContainer, modelChanges, 5, nameof(AppliedMigration.PrimaryKey), "not null primary key");
                AssertColumnConstraints(migrationsContainer, modelChanges, 5, nameof(AppliedMigration.DateTime), "not null");
                AssertColumnConstraints(migrationsContainer, modelChanges, 5, nameof(AppliedMigration.CommandText), "not null");
                AssertColumnConstraints(migrationsContainer, modelChanges, 5, nameof(AppliedMigration.Name), "not null");

                AssertCreateTable(modelChanges, 6, typeof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeader));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 6, nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeader.PrimaryKey), "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 6, "Value_SystemType_Assembly", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 6, "Value_SystemType_Type", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 6, "Value_Value", "not null");

                AssertCreateTable(modelChanges, 7, typeof(Blog));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 7, nameof(Blog.PrimaryKey), "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 7, nameof(Blog.Theme), "not null");
                AssertMtmColumn(tracingEndpointContainer, modelChanges, 7, "Posts_Left");

                AssertCreateTable(modelChanges, 8, typeof(Community));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 8, nameof(Community.PrimaryKey), "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 8, nameof(Community.Name), "not null");
                AssertMtmColumn(tracingEndpointContainer, modelChanges, 8, "Participants_Left");

                AssertCreateTable(modelChanges, 9, typeof(Participant));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 9, nameof(Participant.PrimaryKey), "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 9, nameof(Participant.Name), "not null");
                AssertMtmColumn(tracingEndpointContainer, modelChanges, 9, "Communities_Right");

                AssertCreateTable(modelChanges, 10, typeof(User));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 10, nameof(User.PrimaryKey), "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 10, nameof(User.Nickname), "not null");

                AssertCreateTable(modelChanges, 11, typeof(TracingEndpoint.DatabaseModel.IntegrationMessage));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 11, nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.PrimaryKey), "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 11, nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.MessageId), "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 11, nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.ConversationId), "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 11, nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.InitiatorMessageId), string.Empty);
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 11, "Payload_SystemType_Assembly", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 11, "Payload_SystemType_Type", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 11, "Payload_Value", "not null");

                AssertCreateTable(modelChanges, 12, typeof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessage));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 12, nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessage.PrimaryKey), "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 12, "Payload_SystemType_Assembly", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 12, "Payload_SystemType_Type", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 12, "Payload_Value", "not null");
                AssertMtmColumn(tracingEndpointContainer, modelChanges, 12, "Headers_Left");

                AssertCreateMtmTable(modelChanges, 13, "SpaceEngineersCoreModulesTest", "Community_Participant");
                AssertHasNoColumn(tracingEndpointContainer, modelChanges, 13, nameof(IDatabaseEntity<Guid>.PrimaryKey));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 13, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), @"not null references ""SpaceEngineersCoreModulesTest"".""Community"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 13, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), @"not null references ""SpaceEngineersCoreModulesTest"".""Participant"" (""PrimaryKey"")");

                AssertCreateTable(modelChanges, 14, typeof(Post));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 14, nameof(Post.PrimaryKey), "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 14, nameof(Post.DateTime), "not null");
                AssertHasNoColumn(tracingEndpointContainer, modelChanges, 14, "Blog_Right");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 14, "Blog_PrimaryKey", @"not null references ""SpaceEngineersCoreModulesTest"".""Blog"" (""PrimaryKey"")");

                AssertCreateTable(modelChanges, 15, typeof(CapturedMessage));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 15, "PrimaryKey", "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 15, "Message_PrimaryKey", @"not null references ""SpaceEngineersCoreTracingEndpoint"".""IntegrationMessage"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 15, nameof(CapturedMessage.RefuseReason), string.Empty);

                AssertCreateTable(modelChanges, 16, typeof(InboxMessage));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 16, nameof(InboxMessage.PrimaryKey), "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 16, "Message_PrimaryKey", @"not null references ""SpaceEngineersCoreGenericEndpointDataAccess"".""IntegrationMessage"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 16, "EndpointIdentity_LogicalName", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 16, "EndpointIdentity_InstanceName", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 16, nameof(InboxMessage.IsError), "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 16, nameof(InboxMessage.Handled), "not null");

                AssertCreateMtmTable(modelChanges, 17, "SpaceEngineersCoreGenericEndpointDataAccess", "IntegrationMessage_IntegrationMessageHeader");
                AssertHasNoColumn(tracingEndpointContainer, modelChanges, 17, nameof(IDatabaseEntity<Guid>.PrimaryKey));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 17, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), @"not null references ""SpaceEngineersCoreGenericEndpointDataAccess"".""IntegrationMessage"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 17, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), @"not null references ""SpaceEngineersCoreGenericEndpointDataAccess"".""IntegrationMessageHeader"" (""PrimaryKey"")");

                AssertCreateTable(modelChanges, 18, typeof(OutboxMessage));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 18, nameof(OutboxMessage.PrimaryKey), "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 18, "Message_PrimaryKey", @"not null references ""SpaceEngineersCoreGenericEndpointDataAccess"".""IntegrationMessage"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 18, nameof(OutboxMessage.Sent), "not null");

                AssertCreateMtmTable(modelChanges, 19, "SpaceEngineersCoreModulesTest", "Blog_Post");
                AssertHasNoColumn(tracingEndpointContainer, modelChanges, 19, nameof(IDatabaseEntity<Guid>.PrimaryKey));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 19, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), @"not null references ""SpaceEngineersCoreModulesTest"".""Blog"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 19, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), @"not null references ""SpaceEngineersCoreModulesTest"".""Post"" (""PrimaryKey"")");

                AssertCreateView(modelChanges, 20, nameof(DatabaseColumn));
                AssertCreateView(modelChanges, 21, nameof(DatabaseColumnConstraint));
                AssertCreateView(modelChanges, 22, nameof(DatabaseIndex));
                AssertCreateView(modelChanges, 23, nameof(DatabaseSchema));
                AssertCreateView(modelChanges, 24, nameof(DatabaseView));

                AssertCreateIndex(modelChanges, 25, "SpaceEngineersCoreDataAccessOrmSqlHost", nameof(AppliedMigration), nameof(AppliedMigration.Name));
                AssertCreateIndex(modelChanges, 26, "SpaceEngineersCoreDataAccessOrmSqlHost", nameof(DatabaseColumn), "Column_Schema_Table");
                AssertCreateIndex(modelChanges, 27, "SpaceEngineersCoreDataAccessOrmSqlHost", nameof(DatabaseIndex), "Index_Schema_Table");
                AssertCreateIndex(modelChanges, 28, "SpaceEngineersCoreDataAccessOrmSqlHost", nameof(DatabaseSchema), "Name");
                AssertCreateIndex(modelChanges, 29, "SpaceEngineersCoreDataAccessOrmSqlHost", nameof(DatabaseView), "Query_Schema_View");
                AssertCreateIndex(modelChanges, 30, "SpaceEngineersCoreGenericEndpointDataAccess", "IntegrationMessage_IntegrationMessageHeader", "Left_Right");
                AssertCreateIndex(modelChanges, 31, "SpaceEngineersCoreModulesTest", "Blog_Post", "Left_Right");
                AssertCreateIndex(modelChanges, 32, "SpaceEngineersCoreModulesTest", "Community_Participant", "Left_Right");

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
                    Assert.True(modelProvider.TablesMap.ContainsKey(createTable.Schema));
                    Assert.True(modelProvider.TablesMap[createTable.Schema].ContainsKey(createTable.Table));
                    Assert.True(modelProvider.TablesMap[createTable.Schema][createTable.Table] is TableInfo);
                    var tableInfo = (TableInfo)modelProvider.TablesMap[createTable.Schema][createTable.Table];
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
                    Assert.True(modelProvider.TablesMap.ContainsKey(createTable.Schema));
                    Assert.True(modelProvider.TablesMap[createTable.Schema].ContainsKey(createTable.Table));
                    Assert.True(modelProvider.TablesMap[createTable.Schema][createTable.Table] is TableInfo);
                    var tableInfo = (TableInfo)modelProvider.TablesMap[createTable.Schema][createTable.Table];
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
                    Assert.True(modelProvider.TablesMap.ContainsKey(createTable.Schema));
                    Assert.True(modelProvider.TablesMap[createTable.Schema].ContainsKey(createTable.Table));
                    Assert.True(modelProvider.TablesMap[createTable.Schema][createTable.Table] is TableInfo);
                    var tableInfo = (TableInfo)modelProvider.TablesMap[createTable.Schema][createTable.Table];
                    Assert.True(tableInfo.Columns.Keys.All(key => !key.Contains(column, StringComparison.OrdinalIgnoreCase)));
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

            static void AssertCreateIndex(IModelChange[] modelChanges, int index, string schema, string table, string indexName)
            {
                Assert.True(modelChanges[index] is CreateIndex);
                var createIndex = (CreateIndex)modelChanges[index];
                Assert.True(createIndex.Schema.Equals(schema, StringComparison.OrdinalIgnoreCase));
                Assert.True(createIndex.Table.Equals(table, StringComparison.OrdinalIgnoreCase));
                Assert.True(createIndex.Index.Equals(indexName, StringComparison.OrdinalIgnoreCase));
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostWithDataAccessAndIntegrationTransportTracingTestData))]
        internal async Task GetConversationTraceTest(
            Func<ITestOutputHelper, IHostBuilder, IHostBuilder> useTransport,
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

            var manualMigrations = new[]
            {
                typeof(RecreatePostgreSqlDatabaseManualMigration)
            };

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(Output),
                new TestSettingsScopeProviderOverride(nameof(GetConversationTraceTest))
            };

            var host = useTransport(Output, Host.CreateDefaultBuilder())
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .WithDataAccess(databaseProvider)
                       .WithTracing()
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithOverrides(overrides))
                       .BuildOptions())
               .UseTracingEndpoint(TestIdentity.Instance0,
                    builder => builder
                       .WithDataAccess(databaseProvider)
                       .ModifyContainerOptions(options => options
                           .WithOverrides(overrides))
                       .BuildOptions())
               .ExecuteMigrations(builder => builder
                   .WithDataAccess(databaseProvider)
                   .ModifyContainerOptions(options => options
                       .WithAdditionalOurTypes(manualMigrations)
                       .WithOverrides(overrides))
                   .BuildOptions())
               .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();
            var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();
            var collector = transportDependencyContainer.Resolve<MessagesCollector>();

            var jsonSerializer = host
               .GetEndpointDependencyContainer(TestIdentity.Endpoint10)
               .Resolve<IJsonSerializer>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var conversationId = Guid.NewGuid();

                var awaiter = Task.WhenAll(
                    collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(GetConversationTrace)),
                    collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(ConversationTrace)));

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
                Assert.Equal(typeof(GetConversationTrace), ((CaptureTrace)messages[2].Payload).SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType);
                Assert.Equal(typeof(CaptureTrace), messages[3].ReflectedType);
                Assert.Equal(typeof(ConversationTrace), ((CaptureTrace)messages[3].Payload).SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType);

                Assert.Equal(conversationId, trace.ConversationId);
                Assert.Null(trace.SerializedMessage);
                Assert.Null(trace.RefuseReason);
                Assert.NotNull(trace.SubsequentTrace);
                Assert.Empty(trace.SubsequentTrace);

                awaiter = Task.WhenAll(
                    collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(Query)),
                    collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(Reply)));

                var reply = await integrationContext
                    .RpcRequest<Query, Reply>(new Query(42), cts.Token)
                    .ConfigureAwait(false);

                await awaiter.ConfigureAwait(false);

                Assert.Empty(collector.ErrorMessages);
                Assert.Equal(4, collector.Messages.Count);
                messages = collector.Messages.ToArray();
                collector.Messages.Clear();

                Assert.Equal(typeof(Query), messages[0].ReflectedType);
                Assert.Equal(typeof(Reply), messages[1].ReflectedType);
                Assert.Equal(typeof(CaptureTrace), messages[2].ReflectedType);
                Assert.Equal(typeof(Query), ((CaptureTrace)messages[2].Payload).SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType);
                Assert.Equal(typeof(CaptureTrace), messages[3].ReflectedType);
                Assert.Equal(typeof(Reply), ((CaptureTrace)messages[3].Payload).SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType);

                conversationId = messages[0].ReadRequiredHeader<ConversationId>().Value;

                awaiter = Task.WhenAll(
                    collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(GetConversationTrace)),
                    collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(ConversationTrace)));

                trace = await integrationContext
                    .RpcRequest<GetConversationTrace, ConversationTrace>(new GetConversationTrace(conversationId), cts.Token)
                    .ConfigureAwait(false);

                await awaiter.ConfigureAwait(false);

                Assert.Empty(collector.ErrorMessages);
                Assert.Equal(4, collector.Messages.Count);
                collector.Messages.Clear();

                Assert.Equal(conversationId, trace.ConversationId);
                Assert.NotNull(trace.SerializedMessage);
                Assert.NotEmpty(trace.SerializedMessage.Payload);
                Assert.Equal(typeof(Query), trace.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType);
                Assert.Equal(42, ((Query)trace.SerializedMessage.ToIntegrationMessage(jsonSerializer).Payload).Id);
                Assert.Null(trace.RefuseReason);
                Assert.NotNull(trace.SubsequentTrace);
                Assert.Single(trace.SubsequentTrace);

                var subsequentTrace = trace.SubsequentTrace.Single();
                Assert.NotNull(subsequentTrace.SerializedMessage);
                Assert.NotEmpty(subsequentTrace.SerializedMessage.Payload);
                Assert.Equal(typeof(Reply), subsequentTrace.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType);
                Assert.Equal(reply.Id, ((Reply)subsequentTrace.SerializedMessage.ToIntegrationMessage(jsonSerializer).Payload).Id);
                Assert.Null(subsequentTrace.RefuseReason);
                Assert.NotNull(subsequentTrace.SubsequentTrace);
                Assert.Empty(subsequentTrace.SubsequentTrace);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }
    }
}