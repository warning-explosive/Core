namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using CrossCuttingConcerns.Json;
    using DataAccess.Api.Model;
    using DataAccess.Orm.Connection;
    using DataAccess.Orm.Host;
    using DataAccess.Orm.Host.Model;
    using DataAccess.Orm.PostgreSql.Host;
    using DataAccess.Orm.Sql.Host.Model;
    using DataAccess.Orm.Sql.Model;
    using DatabaseEntities.Relations;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.DataAccess.DatabaseModel;
    using GenericEndpoint.DataAccess.EventSourcing;
    using GenericEndpoint.Host;
    using GenericEndpoint.Messaging.MessageHeaders;
    using IntegrationTransport.Host;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Migrations;
    using Mocks;
    using Overrides;
    using Registrations;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
    using TracingEndpoint.Contract;
    using TracingEndpoint.DatabaseModel;
    using TracingEndpoint.Host;
    using Xunit;
    using Xunit.Abstractions;
    using EndpointIdentity = GenericEndpoint.Contract.EndpointIdentity;
    using User = DatabaseEntities.Relations.User;

    /// <summary>
    /// GenericHost assembly tests
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
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
            var commonAppSettingsJson = SolutionExtensions
               .ProjectFile()
               .Directory
               .EnsureNotNull("Project directory not found")
               .StepInto("Settings")
               .GetFile("appsettings", ".json")
               .FullName;

            var useInMemoryIntegrationTransport = new Func<string, ILogger, IHostBuilder, IHostBuilder>(
                (test, logger, hostBuilder) => hostBuilder
                   .ConfigureAppConfiguration(builder => builder.AddJsonFile(commonAppSettingsJson))
                   .UseIntegrationTransport(builder => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .ModifyContainerOptions(options => options
                           .WithOverrides(new TestLoggerOverride(logger))
                           .WithOverrides(new TestSettingsScopeProviderOverride(test)))
                       .BuildOptions()));

            var useRabbitMqIntegrationTransport = new Func<string, ILogger, IHostBuilder, IHostBuilder>(
                (test, logger, hostBuilder) => hostBuilder
                   .ConfigureAppConfiguration(builder => builder.AddJsonFile(commonAppSettingsJson))
                   .UseIntegrationTransport(builder => builder
                       .WithRabbitMqIntegrationTransport(hostBuilder)
                       .ModifyContainerOptions(options => options
                           .WithOverrides(new TestLoggerOverride(logger))
                           .WithOverrides(new TestSettingsScopeProviderOverride(test)))
                       .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                useInMemoryIntegrationTransport,
                useRabbitMqIntegrationTransport
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

            var commonAppSettingsJson = SolutionExtensions
               .ProjectFile()
               .Directory
               .EnsureNotNull("Project directory not found")
               .StepInto("Settings")
               .GetFile("appsettings", ".json")
               .FullName;

            var useInMemoryIntegrationTransport = new Func<string, ILogger, IHostBuilder, IHostBuilder>(
                (test, logger, hostBuilder) => hostBuilder
                   .ConfigureAppConfiguration(builder => builder.AddJsonFile(commonAppSettingsJson))
                   .UseIntegrationTransport(builder => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .ModifyContainerOptions(options => options
                           .WithManualRegistrations(new MessagesCollectorManualRegistration())
                           .WithManualRegistrations(new AnonymousUserScopeProviderManualRegistration())
                           .WithOverrides(new TestLoggerOverride(logger))
                           .WithOverrides(new TestSettingsScopeProviderOverride(test)))
                       .BuildOptions()));

            var useRabbitMqIntegrationTransport = new Func<string, ILogger, IHostBuilder, IHostBuilder>(
                (test, logger, hostBuilder) => hostBuilder
                   .ConfigureAppConfiguration(builder => builder.AddJsonFile(commonAppSettingsJson))
                   .UseIntegrationTransport(builder => builder
                       .WithRabbitMqIntegrationTransport(hostBuilder)
                       .ModifyContainerOptions(options => options
                           .WithManualRegistrations(new MessagesCollectorManualRegistration())
                           .WithManualRegistrations(new AnonymousUserScopeProviderManualRegistration())
                           .WithOverrides(new TestLoggerOverride(logger))
                           .WithOverrides(new TestSettingsScopeProviderOverride(test)))
                       .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                useInMemoryIntegrationTransport,
                useRabbitMqIntegrationTransport
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

            var commonAppSettingsJson = SolutionExtensions
               .ProjectFile()
               .Directory
               .EnsureNotNull("Project directory not found")
               .StepInto("Settings")
               .GetFile("appsettings", ".json")
               .FullName;

            var useInMemoryIntegrationTransport = new Func<string, ILogger, IHostBuilder, IHostBuilder>(
                (test, logger, hostBuilder) => hostBuilder
                   .ConfigureAppConfiguration(builder => builder.AddJsonFile(commonAppSettingsJson))
                   .UseIntegrationTransport(builder => builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .WithTracing()
                       .ModifyContainerOptions(options => options
                           .WithManualRegistrations(new MessagesCollectorManualRegistration())
                           .WithManualRegistrations(new AnonymousUserScopeProviderManualRegistration())
                           .WithOverrides(new TestLoggerOverride(logger))
                           .WithOverrides(new TestSettingsScopeProviderOverride(test)))
                       .BuildOptions()));

            var useRabbitMqIntegrationTransport = new Func<string, ILogger, IHostBuilder, IHostBuilder>(
                (test, logger, hostBuilder) => hostBuilder
                   .ConfigureAppConfiguration(builder => builder.AddJsonFile(commonAppSettingsJson))
                   .UseIntegrationTransport(builder => builder
                       .WithRabbitMqIntegrationTransport(hostBuilder)
                       .WithTracing()
                       .ModifyContainerOptions(options => options
                           .WithManualRegistrations(new MessagesCollectorManualRegistration())
                           .WithManualRegistrations(new AnonymousUserScopeProviderManualRegistration())
                           .WithOverrides(new TestLoggerOverride(logger))
                           .WithOverrides(new TestSettingsScopeProviderOverride(test)))
                       .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                useInMemoryIntegrationTransport,
                useRabbitMqIntegrationTransport
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
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            IDatabaseProvider databaseProvider)
        {
            Output.WriteLine(databaseProvider.GetType().FullName);

            var logger = Fixture.CreateLogger(Output);

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(nameof(CompareEquivalentDatabaseDatabaseModelsTest))
            };

            var host = useTransport(nameof(CompareEquivalentDatabaseDatabaseModelsTest), logger, Fixture.CreateHostBuilder(Output))
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
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            IDatabaseProvider databaseProvider)
        {
            Output.WriteLine(databaseProvider.GetType().FullName);

            var logger = Fixture.CreateLogger(Output);

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
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(nameof(ExtractDatabaseModelChangesDiffTest))
            };

            var host = useTransport(nameof(ExtractDatabaseModelChangesDiffTest), logger, Fixture.CreateHostBuilder(Output))
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
                Assert.Equal(35, modelChanges.Length);

                AssertCreateDataBase(modelChanges, 0, nameof(ExtractDatabaseModelChangesDiffTest));

                AssertCreateSchema(modelChanges, 1, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(DataAccess), nameof(DataAccess.Orm), nameof(DataAccess.Orm.Sql), nameof(DataAccess.Orm.Sql.Host)));
                AssertCreateSchema(modelChanges, 2, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.DataAccess)));
                AssertCreateSchema(modelChanges, 3, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericHost), nameof(Core.GenericHost.Test)));
                AssertCreateSchema(modelChanges, 4, string.Join(string.Empty, nameof(SpaceEngineers), nameof(Core), nameof(TracingEndpoint)));

                new Action<int>(index =>
                {
                    AssertCreateTable(modelChanges, index, typeof(AppliedMigration));
                    AssertColumnConstraints(migrationsContainer, modelChanges, index, nameof(AppliedMigration.PrimaryKey), "not null primary key");
                    AssertColumnConstraints(migrationsContainer, modelChanges, index, nameof(AppliedMigration.DateTime), "not null");
                    AssertColumnConstraints(migrationsContainer, modelChanges, index, nameof(AppliedMigration.CommandText), "not null");
                    AssertColumnConstraints(migrationsContainer, modelChanges, index, nameof(AppliedMigration.Name), "not null");
                })(5);

                new Action<int>(index =>
                {
                    AssertCreateTable(modelChanges, index, typeof(DatabaseDomainEvent));
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(DatabaseDomainEvent.PrimaryKey), "not null primary key");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(DatabaseDomainEvent.AggregateId), "not null");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(DatabaseDomainEvent.Index), "not null");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(DatabaseDomainEvent.Timestamp), "not null");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(DatabaseDomainEvent.EventType)}_{nameof(SystemType.Assembly)}", "not null");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(DatabaseDomainEvent.EventType)}_{nameof(SystemType.Type)}", "not null");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(DatabaseDomainEvent.SerializedEvent), "not null");
                })(6);

                new Action<int>(index =>
                {
                    AssertCreateTable(modelChanges, index, typeof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeader));
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeader.PrimaryKey), "not null primary key");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeader.Value)}_{nameof(JsonObject.SystemType)}_{nameof(SystemType.Assembly)}", "not null");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeader.Value)}_{nameof(JsonObject.SystemType)}_{nameof(SystemType.Type)}", "not null");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeader.Value)}_{nameof(JsonObject.Value)}", "not null");
                })(7);

                new Action<int>(index =>
                {
                    AssertCreateTable(modelChanges, index, typeof(Blog));
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Blog.PrimaryKey), "not null primary key");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Blog.Theme), "not null");
                    AssertMtmColumn(tracingEndpointContainer, modelChanges, index, $"{nameof(Blog.Posts)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}");
                })(8);

                new Action<int>(index =>
                {
                    AssertCreateTable(modelChanges, index, typeof(Community));
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Community.PrimaryKey), "not null primary key");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Community.Name), "not null");
                    AssertMtmColumn(tracingEndpointContainer, modelChanges, index, $"{nameof(Community.Participants)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}");
                })(9);

                new Action<int>(index =>
                {
                    AssertCreateTable(modelChanges, index, typeof(Participant));
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Participant.PrimaryKey), "not null primary key");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Participant.Name), "not null");
                    AssertMtmColumn(tracingEndpointContainer, modelChanges, index, $"{nameof(Participant.Communities)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right)}");
                })(10);

                new Action<int>(index =>
                {
                    AssertCreateTable(modelChanges, index, typeof(User));
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(User.PrimaryKey), "not null primary key");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(User.Nickname), "not null");
                })(11);

                new Action<int>(index =>
                {
                    AssertCreateTable(modelChanges, index, typeof(TracingEndpoint.DatabaseModel.IntegrationMessage));
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.PrimaryKey), "not null primary key");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.MessageId), "not null");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.ConversationId), "not null");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.InitiatorMessageId), string.Empty);
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.Payload)}_{nameof(JsonObject.SystemType)}_{nameof(SystemType.Assembly)}", "not null");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.Payload)}_{nameof(JsonObject.SystemType)}_{nameof(SystemType.Type)}", "not null");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(TracingEndpoint.DatabaseModel.IntegrationMessage.Payload)}_{nameof(JsonObject.Value)}", "not null");
                })(12);

                new Action<int>(index =>
                {
                    AssertCreateTable(modelChanges, index, typeof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessage));
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessage.PrimaryKey), "not null primary key");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessage.Payload)}_{nameof(JsonObject.SystemType)}_{nameof(SystemType.Assembly)}", "not null");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessage.Payload)}_{nameof(JsonObject.SystemType)}_{nameof(SystemType.Type)}", "not null");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessage.Payload)}_{nameof(JsonObject.Value)}", "not null");
                    AssertMtmColumn(tracingEndpointContainer, modelChanges, index, $"{nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessage.Headers)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}");
                })(13);

                new Action<int>(index =>
                {
                    AssertCreateMtmTable(modelChanges, index, "SpaceEngineersCoreGenericHostTest", $"{nameof(Community)}_{nameof(Participant)}");
                    AssertHasNoColumn(tracingEndpointContainer, modelChanges, index, nameof(IDatabaseEntity<Guid>.PrimaryKey));
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), $@"not null references ""SpaceEngineersCoreGenericHostTest"".""{nameof(Community)}"" (""{nameof(IDatabaseEntity<Guid>.PrimaryKey)}"")");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), $@"not null references ""SpaceEngineersCoreGenericHostTest"".""{nameof(Participant)}"" (""{nameof(IDatabaseEntity<Guid>.PrimaryKey)}"")");
                })(14);

                new Action<int>(index =>
                {
                    AssertCreateTable(modelChanges, index, typeof(Post));
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Post.PrimaryKey), "not null primary key");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(Post.DateTime), "not null");
                    AssertHasNoColumn(tracingEndpointContainer, modelChanges, index, $"{nameof(Blog)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right)}");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(Post.Blog)}_{nameof(Post.Blog.PrimaryKey)}", $@"not null references ""SpaceEngineersCoreGenericHostTest"".""{nameof(Blog)}"" (""{nameof(IDatabaseEntity<Guid>.PrimaryKey)}"")");
                })(15);

                new Action<int>(index =>
                {
                    AssertCreateTable(modelChanges, index, typeof(CapturedMessage));
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(CapturedMessage.PrimaryKey), "not null primary key");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(CapturedMessage.Message)}_{nameof(CapturedMessage.Message.PrimaryKey)}", $@"not null references ""SpaceEngineersCoreTracingEndpoint"".""{nameof(TracingEndpoint.DatabaseModel.IntegrationMessage)}"" (""{nameof(IDatabaseEntity<Guid>.PrimaryKey)}"")");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(CapturedMessage.RefuseReason), string.Empty);
                })(16);

                new Action<int>(index =>
                {
                    AssertCreateTable(modelChanges, index, typeof(InboxMessage));
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(InboxMessage.PrimaryKey), "not null primary key");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(InboxMessage.Message)}_{nameof(InboxMessage.Message.PrimaryKey)}", $@"not null references ""SpaceEngineersCoreGenericEndpointDataAccess"".""{nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessage)}"" (""{nameof(IDatabaseEntity<Guid>.PrimaryKey)}"")");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(InboxMessage.EndpointIdentity)}_{nameof(GenericEndpoint.DataAccess.DatabaseModel.EndpointIdentity.LogicalName)}", "not null");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(InboxMessage.EndpointIdentity)}_{nameof(GenericEndpoint.DataAccess.DatabaseModel.EndpointIdentity.InstanceName)}", "not null");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(InboxMessage.IsError), "not null");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(InboxMessage.Handled), "not null");
                })(17);

                new Action<int>(index =>
                {
                    AssertCreateMtmTable(modelChanges, index, "SpaceEngineersCoreGenericEndpointDataAccess", $"{nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessage)}_{nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeader)}");
                    AssertHasNoColumn(tracingEndpointContainer, modelChanges, index, nameof(IDatabaseEntity<Guid>.PrimaryKey));
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), $@"not null references ""SpaceEngineersCoreGenericEndpointDataAccess"".""{nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessage)}"" (""{nameof(IDatabaseEntity<Guid>.PrimaryKey)}"")");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), $@"not null references ""SpaceEngineersCoreGenericEndpointDataAccess"".""{nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeader)}"" (""{nameof(IDatabaseEntity<Guid>.PrimaryKey)}"")");
                })(18);

                new Action<int>(index =>
                {
                    AssertCreateTable(modelChanges, index, typeof(OutboxMessage));
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(OutboxMessage.PrimaryKey), "not null primary key");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, $"{nameof(OutboxMessage.Message)}_{nameof(OutboxMessage.Message.PrimaryKey)}", $@"not null references ""SpaceEngineersCoreGenericEndpointDataAccess"".""{nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessage)}"" (""{nameof(IDatabaseEntity<Guid>.PrimaryKey)}"")");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(OutboxMessage.Sent), "not null");
                })(19);

                new Action<int>(index =>
                {
                    AssertCreateMtmTable(modelChanges, index, "SpaceEngineersCoreGenericHostTest", $"{nameof(Blog)}_{nameof(Post)}");
                    AssertHasNoColumn(tracingEndpointContainer, modelChanges, index, nameof(IDatabaseEntity<Guid>.PrimaryKey));
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), $@"not null references ""SpaceEngineersCoreGenericHostTest"".""{nameof(Blog)}"" (""{nameof(IDatabaseEntity<Guid>.PrimaryKey)}"")");
                    AssertColumnConstraints(tracingEndpointContainer, modelChanges, index, nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), $@"not null references ""SpaceEngineersCoreGenericHostTest"".""{nameof(Post)}"" (""{nameof(IDatabaseEntity<Guid>.PrimaryKey)}"")");
                })(20);

                AssertCreateView(modelChanges, 21, nameof(DatabaseColumn));
                AssertCreateView(modelChanges, 22, nameof(DatabaseColumnConstraint));
                AssertCreateView(modelChanges, 23, nameof(DatabaseIndex));
                AssertCreateView(modelChanges, 24, nameof(DatabaseSchema));
                AssertCreateView(modelChanges, 25, nameof(DatabaseView));

                AssertCreateIndex(modelChanges, 26, "SpaceEngineersCoreDataAccessOrmSqlHost", nameof(AppliedMigration), nameof(AppliedMigration.Name));
                AssertCreateIndex(modelChanges, 27, "SpaceEngineersCoreDataAccessOrmSqlHost", nameof(DatabaseColumn), $"{nameof(DatabaseColumn.Column)}_{nameof(DatabaseColumn.Schema)}_{nameof(DatabaseColumn.Table)}");
                AssertCreateIndex(modelChanges, 28, "SpaceEngineersCoreDataAccessOrmSqlHost", nameof(DatabaseIndex), $"{nameof(DatabaseIndex.Index)}_{nameof(DatabaseIndex.Schema)}_{nameof(DatabaseIndex.Table)}");
                AssertCreateIndex(modelChanges, 29, "SpaceEngineersCoreDataAccessOrmSqlHost", nameof(DatabaseSchema), $"{nameof(DatabaseSchema.Name)}");
                AssertCreateIndex(modelChanges, 30, "SpaceEngineersCoreDataAccessOrmSqlHost", nameof(DatabaseView), $"{nameof(DatabaseView.Query)}_{nameof(DatabaseView.Schema)}_{nameof(DatabaseView.View)}");
                AssertCreateIndex(modelChanges, 31, "SpaceEngineersCoreGenericEndpointDataAccess", nameof(DatabaseDomainEvent), $"{nameof(DatabaseDomainEvent.AggregateId)}_{nameof(DatabaseDomainEvent.Index)}");
                AssertCreateIndex(modelChanges, 32, "SpaceEngineersCoreGenericEndpointDataAccess", $"{nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessage)}_{nameof(GenericEndpoint.DataAccess.DatabaseModel.IntegrationMessageHeader)}", $"{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right)}");
                AssertCreateIndex(modelChanges, 33, "SpaceEngineersCoreGenericHostTest", $"{nameof(Blog)}_{nameof(Post)}", $"{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right)}");
                AssertCreateIndex(modelChanges, 34, "SpaceEngineersCoreGenericHostTest", $"{nameof(Community)}_{nameof(Participant)}", $"{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left)}_{nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right)}");

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
                Assert.True(createIndex.Index.Equals(string.Join("__", table, indexName), StringComparison.OrdinalIgnoreCase));
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostWithDataAccessAndIntegrationTransportTracingTestData))]
        internal async Task GetConversationTraceTest(
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            IDatabaseProvider databaseProvider,
            TimeSpan timeout)
        {
            Output.WriteLine(databaseProvider.GetType().FullName);

            var logger = Fixture.CreateLogger(Output);

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

            var settingsScope = nameof(GetConversationTraceTest);

            var overrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(settingsScope)
            };

            var host = useTransport(nameof(GetConversationTraceTest), logger, Fixture.CreateHostBuilder(Output))
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
            var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

            var jsonSerializer = host
               .GetEndpointDependencyContainer(TestIdentity.Endpoint10)
               .Resolve<IJsonSerializer>();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var hostShutdown = host.WaitForShutdownAsync(cts.Token);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                var conversationId = Guid.NewGuid();

                var awaiter = Task.WhenAny(
                    hostShutdown,
                    Task.WhenAll(
                        collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(GetConversationTrace)),
                        collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(ConversationTrace))));

                await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                    var trace = await integrationContext
                        .RpcRequest<GetConversationTrace, ConversationTrace>(new GetConversationTrace(conversationId), cts.Token)
                        .ConfigureAwait(false);

                    var result = await awaiter.ConfigureAwait(false);

                    if (hostShutdown == result)
                    {
                        throw new InvalidOperationException("Host was unexpectedly stopped");
                    }

                    await result.ConfigureAwait(false);

                    Assert.Empty(collector.ErrorMessages);
                    Assert.Equal(4, collector.Messages.Count);
                    var messages = collector.Messages.ToArray();
                    collector.Messages.Clear();

                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(GetConversationTrace)));
                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(ConversationTrace)));
                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(CaptureTrace) && ((CaptureTrace)message.Payload).SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(GetConversationTrace)));
                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(CaptureTrace) && ((CaptureTrace)message.Payload).SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(ConversationTrace)));

                    Assert.Equal(conversationId, trace.ConversationId);
                    Assert.Null(trace.SerializedMessage);
                    Assert.Null(trace.RefuseReason);
                    Assert.NotNull(trace.SubsequentTrace);
                    Assert.Empty(trace.SubsequentTrace);

                    awaiter = Task.WhenAny(
                        hostShutdown,
                        Task.WhenAll(
                            collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(Query)),
                            collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(Reply))));

                    var reply = await integrationContext
                        .RpcRequest<Query, Reply>(new Query(42), cts.Token)
                        .ConfigureAwait(false);

                    result = await awaiter.ConfigureAwait(false);

                    if (hostShutdown == result)
                    {
                        throw new InvalidOperationException("Host was unexpectedly stopped");
                    }

                    await result.ConfigureAwait(false);

                    Assert.Empty(collector.ErrorMessages);
                    Assert.Equal(4, collector.Messages.Count);
                    messages = collector.Messages.ToArray();
                    collector.Messages.Clear();

                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(Query)));
                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(Reply)));
                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(CaptureTrace) && ((CaptureTrace)message.Payload).SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(Query)));
                    Assert.Single(messages.Where(message => message.ReflectedType == typeof(CaptureTrace) && ((CaptureTrace)message.Payload).SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(Reply)));

                    conversationId = messages[0].ReadRequiredHeader<ConversationId>().Value;

                    awaiter = Task.WhenAny(
                        hostShutdown,
                        Task.WhenAll(
                            collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(GetConversationTrace)),
                            collector.WaitUntilMessageIsNotReceived<CaptureTrace>(message => message.SerializedMessage.ToIntegrationMessage(jsonSerializer).ReflectedType == typeof(ConversationTrace))));

                    trace = await integrationContext
                        .RpcRequest<GetConversationTrace, ConversationTrace>(new GetConversationTrace(conversationId), cts.Token)
                        .ConfigureAwait(false);

                    result = await awaiter.ConfigureAwait(false);

                    if (hostShutdown == result)
                    {
                        throw new InvalidOperationException("Host was unexpectedly stopped");
                    }

                    await result.ConfigureAwait(false);

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

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(RunHostWithDataAccessTestData))]
        internal async Task BackgroundOutboxDeliveryTest(
            Func<string, ILogger, IHostBuilder, IHostBuilder> useTransport,
            IDatabaseProvider databaseProvider,
            TimeSpan timeout)
        {
            Output.WriteLine(databaseProvider.GetType().FullName);

            var logger = Fixture.CreateLogger(Output);

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

            var settingsScope = nameof(BackgroundOutboxDeliveryTest);

            var endpointManualRegistrations = new IManualRegistration[]
            {
                new BackgroundOutboxDeliveryManualRegistration()
            };

            var endpointOverridesList = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(settingsScope)
            };

            var endpointOverrides = endpointOverridesList.ToArray();

            var migrationOverrides = new IComponentsOverride[]
            {
                new TestLoggerOverride(logger),
                new TestSettingsScopeProviderOverride(settingsScope)
            };

            var host = useTransport(
                    nameof(BackgroundOutboxDeliveryTest),
                    logger,
                    Fixture.CreateHostBuilder(Output))
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => builder
                       .WithDataAccess(databaseProvider)
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithManualRegistrations(endpointManualRegistrations)
                           .WithOverrides(endpointOverrides))
                       .BuildOptions())
               .ExecuteMigrations(builder => builder
                   .WithDataAccess(databaseProvider)
                   .ModifyContainerOptions(options => options
                       .WithAdditionalOurTypes(manualMigrations)
                       .WithOverrides(migrationOverrides))
                   .BuildOptions())
               .BuildHost();

            var transportDependencyContainer = host.GetTransportDependencyContainer();

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var waitUntilTransportIsNotRunning = host.WaitUntilTransportIsNotRunning(Output.WriteLine);

                await host.StartAsync(cts.Token).ConfigureAwait(false);

                var hostShutdown = host.WaitForShutdownAsync(cts.Token);

                await waitUntilTransportIsNotRunning.ConfigureAwait(false);

                Reply reply;

                await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                    var awaiter = Task.WhenAny(
                        hostShutdown,
                        integrationContext.RpcRequest<Query, Reply>(new Query(42), cts.Token));

                    var result = await awaiter.ConfigureAwait(false);

                    if (hostShutdown == result)
                    {
                        throw new InvalidOperationException("Host was unexpectedly stopped");
                    }

                    reply = await ((Task<Reply>)result).ConfigureAwait(false);
                }

                Assert.Equal(42, reply.Id);

                await host.StopAsync(cts.Token).ConfigureAwait(false);
            }
        }
    }
}