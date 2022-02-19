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
    using DataAccess.Orm.Model;
    using DataAccess.Orm.PostgreSql;
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
        /// useContainer
        /// </summary>
        /// <returns>DependencyContainerImplementations</returns>
        public static IEnumerable<Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>>> DependencyContainerImplementations()
        {
            var dependencyContainerProducers = new[]
            {
                new Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>>(options => options.UseGenericContainer())
            };

            return dependencyContainerProducers;
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
            var useInMemoryIntegrationTransport =
                new Func<Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>>, Func<ITestOutputHelper, IHostBuilder, IHostBuilder>>(
                    useContainer => (output, hostBuilder) => hostBuilder
                        .UseIntegrationTransport(builder => builder
                            .WithContainer(useContainer)
                            .WithInMemoryIntegrationTransport(hostBuilder)
                            .WithDefaultCrossCuttingConcerns()
                            .ModifyContainerOptions(options => options
                                .WithOverrides(new TestLoggerOverride(output))
                                .WithOverrides(new TestSettingsScopeProviderOverride(nameof(ExtractDatabaseModelChangesDiffTest))))
                            .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                useInMemoryIntegrationTransport
            };

            return DependencyContainerImplementations()
                .SelectMany(useContainer => integrationTransportProviders
                    .SelectMany(useTransport => DatabaseProviders()
                        .Select(databaseProvider => new object[]
                        {
                            useContainer,
                            useTransport(useContainer),
                            databaseProvider
                        })));
        }

        /// <summary>
        /// useContainer; useTransport; collector; databaseProvider; timeout;
        /// </summary>
        /// <returns>RunHostWithDataAccessTestData</returns>
        public static IEnumerable<object[]> RunHostWithDataAccessTestData()
        {
            var timeout = TimeSpan.FromSeconds(60);

            var useInMemoryIntegrationTransport =
                new Func<Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>>, Func<ITestOutputHelper, IHostBuilder, IHostBuilder>>(
                    useContainer => (output, hostBuilder) => hostBuilder
                        .UseIntegrationTransport(builder => builder
                            .WithContainer(useContainer)
                            .WithInMemoryIntegrationTransport(hostBuilder)
                            .WithDefaultCrossCuttingConcerns()
                            .ModifyContainerOptions(options => options
                                .WithManualRegistrations(new MessagesCollectorManualRegistration())
                                .WithOverrides(new TestLoggerOverride(output))
                                .WithOverrides(new TestSettingsScopeProviderOverride(nameof(ExtractDatabaseModelChangesDiffTest))))
                            .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                useInMemoryIntegrationTransport
            };

            return DependencyContainerImplementations()
                .SelectMany(useContainer => integrationTransportProviders
                    .SelectMany(useTransport => DatabaseProviders()
                        .Select(databaseProvider => new object[]
                            {
                                useContainer,
                                useTransport(useContainer),
                                databaseProvider,
                                timeout
                            })));
        }

        /// <summary>
        /// useContainer; useTransport; collector; databaseProvider; timeout;
        /// </summary>
        /// <returns>RunHostWithDataAccessTestData</returns>
        public static IEnumerable<object[]> RunHostWithDataAccessAndIntegrationTransportTracingTestData()
        {
            var timeout = TimeSpan.FromSeconds(60);

            var useInMemoryIntegrationTransport =
                new Func<Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>>, Func<ITestOutputHelper, IHostBuilder, IHostBuilder>>(
                    useContainer => (output, hostBuilder) => hostBuilder
                        .UseIntegrationTransport(builder => builder
                            .WithContainer(useContainer)
                            .WithInMemoryIntegrationTransport(hostBuilder)
                            .WithDefaultCrossCuttingConcerns()
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

            return DependencyContainerImplementations()
                .SelectMany(useContainer => integrationTransportProviders
                    .SelectMany(useTransport => DatabaseProviders()
                        .Select(databaseProvider => new object[]
                            {
                                useContainer,
                                useTransport(useContainer),
                                databaseProvider,
                                timeout
                            })));
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostWithDataAccessTestData))]
        internal async Task CompareEquivalentDatabaseDatabaseModelsTest(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
            Func<ITestOutputHelper, IHostBuilder, IHostBuilder> useTransport,
            IDatabaseProvider databaseProvider)
        {
            Output.WriteLine(databaseProvider.GetType().FullName);

            var host = useTransport(Output, Host.CreateDefaultBuilder())
                .UseTracingEndpoint(
                    TestIdentity.Instance0,
                    builder => builder
                        .WithContainer(useContainer)
                        .WithDefaultCrossCuttingConcerns()
                        .WithDataAccess(databaseProvider)
                        .ModifyContainerOptions(options => options
                            .WithOverrides(new TestLoggerOverride(Output))
                            .WithOverrides(new TestSettingsScopeProviderOverride(nameof(CompareEquivalentDatabaseDatabaseModelsTest))))
                        .BuildOptions())
                .BuildHost();

            var tracingEndpointContainer = host.GetEndpointDependencyContainer(
                new EndpointIdentity(TracingEndpointIdentity.LogicalName, TestIdentity.Instance0));

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

            var host = useTransport(Output, Host.CreateDefaultBuilder())
                .UseTracingEndpoint(
                    TestIdentity.Instance0,
                    builder => builder
                        .WithContainer(useContainer)
                        .WithDefaultCrossCuttingConcerns()
                        .WithDataAccess(databaseProvider)
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(additionalOurTypes)
                            .WithOverrides(new TestLoggerOverride(Output))
                            .WithOverrides(new TestSettingsScopeProviderOverride(nameof(ExtractDatabaseModelChangesDiffTest))))
                        .BuildOptions())
                .BuildHost();

            var tracingEndpointContainer = host.GetEndpointDependencyContainer(
                new EndpointIdentity(TracingEndpointIdentity.LogicalName, TestIdentity.Instance0));

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
                Assert.Equal(36, modelChanges.Length);

                AssertCreateDataBase(modelChanges, 0, nameof(ExtractDatabaseModelChangesDiffTest));

                AssertCreateSchema(modelChanges, 1, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(Core.DataAccess), nameof(Core.DataAccess.Orm), nameof(Core.DataAccess.Orm.Sql)));
                AssertCreateSchema(modelChanges, 2, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.DataAccess)));
                AssertCreateSchema(modelChanges, 3, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(Core.Modules), nameof(Core.Modules.Test)));
                AssertCreateSchema(modelChanges, 4, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(Core.TracingEndpoint)));

                AssertCreateTable(modelChanges, 5, typeof(AppliedMigration));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 5, nameof(AppliedMigration.PrimaryKey), "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 5, nameof(AppliedMigration.DateTime), "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 5, nameof(AppliedMigration.CommandText), "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 5, nameof(AppliedMigration.Name), "not null");

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

                AssertCreateTable(modelChanges, 11, typeof(TracingEndpoint.DatabaseModel.IntegrationMessageHeader));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 11, nameof(TracingEndpoint.DatabaseModel.IntegrationMessageHeader.PrimaryKey), "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 11, "Value_SystemType_Assembly", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 11, "Value_SystemType_Type", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 11, "Value_Value", "not null");

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

                AssertCreateTable(modelChanges, 15, typeof(TracingEndpoint.DatabaseModel.IntegrationMessage));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 15, nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.PrimaryKey), "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 15, nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.MessageId), "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 15, nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.ConversationId), "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 15, "Payload_SystemType_Assembly", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 15, "Payload_SystemType_Type", "not null");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 15, "Payload_Value", "not null");
                AssertMtmColumn(tracingEndpointContainer, modelChanges, 15, "Headers_Left");

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

                AssertCreateTable(modelChanges, 20, typeof(CapturedMessage));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 20, "PrimaryKey", "not null primary key");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 20, "Message_PrimaryKey", @"not null references ""SpaceEngineersCoreTracingEndpoint"".""IntegrationMessage"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 20, nameof(CapturedMessage.RefuseReason), string.Empty);

                AssertCreateMtmTable(modelChanges, 21, "SpaceEngineersCoreTracingEndpoint", "IntegrationMessage_IntegrationMessageHeader");
                AssertHasNoColumn(tracingEndpointContainer, modelChanges, 21, nameof(IDatabaseEntity<Guid>.PrimaryKey));
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 21, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), @"not null references ""SpaceEngineersCoreTracingEndpoint"".""IntegrationMessage"" (""PrimaryKey"")");
                AssertColumnConstraints(tracingEndpointContainer, modelChanges, 21, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), @"not null references ""SpaceEngineersCoreTracingEndpoint"".""IntegrationMessageHeader"" (""PrimaryKey"")");

                AssertCreateView(modelChanges, 22, nameof(DatabaseColumn));
                AssertCreateView(modelChanges, 23, nameof(DatabaseColumnConstraint));
                AssertCreateView(modelChanges, 24, nameof(DatabaseIndex));
                AssertCreateView(modelChanges, 25, nameof(DatabaseSchema));
                AssertCreateView(modelChanges, 26, nameof(DatabaseView));

                AssertCreateIndex(modelChanges, 27, "SpaceEngineersCoreDataAccessOrmSql", nameof(AppliedMigration), nameof(AppliedMigration.Name));
                AssertCreateIndex(modelChanges, 28, "SpaceEngineersCoreDataAccessOrmSql", nameof(DatabaseColumn), "Column_Schema_Table");
                AssertCreateIndex(modelChanges, 29, "SpaceEngineersCoreDataAccessOrmSql", nameof(DatabaseIndex), "Index_Schema_Table");
                AssertCreateIndex(modelChanges, 30, "SpaceEngineersCoreDataAccessOrmSql", nameof(DatabaseSchema), "Name");
                AssertCreateIndex(modelChanges, 31, "SpaceEngineersCoreDataAccessOrmSql", nameof(DatabaseView), "Query_Schema_View");
                AssertCreateIndex(modelChanges, 32, "SpaceEngineersCoreGenericEndpointDataAccess", "IntegrationMessage_IntegrationMessageHeader", "Left_Right");
                AssertCreateIndex(modelChanges, 33, "SpaceEngineersCoreModulesTest", "Blog_Post", "Left_Right");
                AssertCreateIndex(modelChanges, 34, "SpaceEngineersCoreModulesTest", "Community_Participant", "Left_Right");
                AssertCreateIndex(modelChanges, 35, "SpaceEngineersCoreTracingEndpoint", "IntegrationMessage_IntegrationMessageHeader", "Left_Right");

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
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> useContainer,
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

            var host = useTransport(Output, Host.CreateDefaultBuilder())
                .UseEndpoint(
                    TestIdentity.Endpoint10,
                    (_, builder) => builder
                        .WithContainer(useContainer)
                        .WithDefaultCrossCuttingConcerns()
                        .WithTracing()
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(additionalOurTypes)
                            .WithOverrides(new TestLoggerOverride(Output))
                            .WithOverrides(new TestSettingsScopeProviderOverride(nameof(GetConversationTraceTest))))
                        .BuildOptions())
                .UseTracingEndpoint(
                    TestIdentity.Instance0,
                    builder => builder
                        .WithContainer(useContainer)
                        .WithDefaultCrossCuttingConcerns()
                        .WithDataAccess(databaseProvider)
                        .ModifyContainerOptions(options => options
                            .WithOverrides(new TestLoggerOverride(Output))
                            .WithOverrides(new TestSettingsScopeProviderOverride(nameof(GetConversationTraceTest))))
                        .BuildOptions())
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

                awaiter = Task.WhenAll(
                    collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.IntegrationMessage.ReflectedType == typeof(Query)),
                    collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.IntegrationMessage.ReflectedType == typeof(Reply)));

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
                Assert.Equal(42, ((Query)trace.Message.Payload).Id);
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