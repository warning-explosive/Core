namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using AuthEndpoint.Contract;
    using AuthEndpoint.Host;
    using Basics;
    using Basics.Primitives;
    using CompositionRoot;
    using CrossCuttingConcerns.Logging;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Orm.Sql.Connection;
    using DataAccess.Orm.Sql.Exceptions;
    using DataAccess.Orm.Sql.Linq;
    using DataAccess.Orm.Sql.Migrations.Model;
    using DataAccess.Orm.Sql.Model;
    using DataAccess.Orm.Sql.Model.Attributes;
    using DataAccess.Orm.Sql.Postgres.Connection;
    using DataAccess.Orm.Sql.Settings;
    using DataAccess.Orm.Sql.Transaction;
    using DatabaseEntities;
    using DatabaseEntities.Relations;
    using Extensions;
    using GenericDomain.EventSourcing.Sql;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Authorization;
    using GenericEndpoint.Authorization.Host;
    using GenericEndpoint.Authorization.Web.Host;
    using GenericEndpoint.DataAccess.Sql.Deduplication;
    using GenericEndpoint.DataAccess.Sql.Host;
    using GenericEndpoint.DataAccess.Sql.Postgres.Host;
    using GenericEndpoint.DataAccess.Sql.Settings;
    using GenericEndpoint.EventSourcing.Host;
    using GenericEndpoint.Host;
    using GenericEndpoint.Host.Builder;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;
    using IntegrationTransport.Api;
    using IntegrationTransport.Api.Abstractions;
    using IntegrationTransport.Host;
    using IntegrationTransport.RabbitMQ;
    using IntegrationTransport.RabbitMQ.Settings;
    using JwtAuthentication;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Mocks;
    using Registrations;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
    using StartupActions;
    using Xunit;
    using Xunit.Abstractions;
    using IntegrationMessage = GenericEndpoint.DataAccess.Sql.Deduplication.IntegrationMessage;
    using User = DatabaseEntities.Relations.User;

    /// <summary>
    /// Endpoint data access tests
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
    public class EndpointDataAccessTests : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public EndpointDataAccessTests(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// Test cases for endpoint data access tests
        /// </summary>
        /// <returns>Test cases</returns>
        public static IEnumerable<object[]> BuildHostEndpointDataAccessTestData()
        {
            var timeout = TimeSpan.FromSeconds(60);

            var projectFileDirectory = SolutionExtensions.ProjectFile().Directory
                                       ?? throw new InvalidOperationException("Project directory wasn't found");

            var settingsDirectory = projectFileDirectory
                .StepInto("Settings")
                .StepInto(nameof(HostBuilderTests));

            var inMemoryIntegrationTransportIdentity = IntegrationTransport.InMemory.Identity.TransportIdentity();

            var useInMemoryIntegrationTransport = new Func<IHostBuilder, TransportIdentity, IHostBuilder>(
                static (hostBuilder, transportIdentity) => hostBuilder.UseInMemoryIntegrationTransport(transportIdentity));

            var rabbitMqIntegrationTransportIdentity = IntegrationTransport.RabbitMQ.Identity.TransportIdentity();

            var useRabbitMqIntegrationTransport = new Func<IHostBuilder, TransportIdentity, IHostBuilder>(
                static (hostBuilder, transportIdentity) => hostBuilder.UseRabbitMqIntegrationTransport(transportIdentity));

            var integrationTransportProviders = new[]
            {
                (inMemoryIntegrationTransportIdentity, useInMemoryIntegrationTransport),
                (rabbitMqIntegrationTransportIdentity, useRabbitMqIntegrationTransport)
            };

            var dataAccessProviders = new Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder>[]
            {
                (builder, dataAccessOptions) => builder.WithPostgreSqlDataAccess(dataAccessOptions)
            };

            var eventSourcingProviders = new Func<IEndpointBuilder, IEndpointBuilder>[]
            {
                builder => builder.WithSqlEventSourcing()
            };

            return integrationTransportProviders
               .SelectMany(transport => dataAccessProviders
                   .SelectMany(withDataAccess => eventSourcingProviders
                       .Select(withEventSourcing =>
                       {
                           var (transportIdentity, useTransport) = transport;

                           return new object[]
                           {
                               settingsDirectory,
                               transportIdentity,
                               useTransport,
                               withDataAccess,
                               withEventSourcing,
                               timeout
                           };
                       })));
        }

        /// <summary>
        /// Test cases for endpoint data access tests
        /// </summary>
        /// <returns>Test cases</returns>
        public static IEnumerable<object[]> EndpointDataAccessTestData()
        {
            Func<string, DirectoryInfo> settingsDirectoryProducer =
                testDirectory =>
                {
                    var projectFileDirectory = SolutionExtensions.ProjectFile().Directory
                                               ?? throw new InvalidOperationException("Project directory wasn't found");

                    return projectFileDirectory
                        .StepInto("Settings")
                        .StepInto(testDirectory);
                };

            var inMemoryIntegrationTransportIdentity = IntegrationTransport.InMemory.Identity.TransportIdentity();

            var useInMemoryIntegrationTransport = new Func<IsolationLevel, Func<IHostBuilder, TransportIdentity, DirectoryInfo, IHostBuilder>>(
                static _ => (hostBuilder, transportIdentity, _) => hostBuilder.UseInMemoryIntegrationTransport(
                    transportIdentity,
                    options => options
                        .WithManualRegistrations(new MessagesCollectorManualRegistration())));

            var rabbitMqIntegrationTransportIdentity = IntegrationTransport.RabbitMQ.Identity.TransportIdentity();

            var useRabbitMqIntegrationTransport = new Func<IsolationLevel, Func<IHostBuilder, TransportIdentity, DirectoryInfo, IHostBuilder>>(
                static isolationLevel => (hostBuilder, transportIdentity, settingsDirectory) => hostBuilder.UseRabbitMqIntegrationTransport(
                    transportIdentity,
                    builder => builder
                        .WithManualRegistrations(new PurgeRabbitMqQueuesManualRegistration())
                        .WithManualRegistrations(new MessagesCollectorManualRegistration())
                        .WithManualRegistrations(new VirtualHostManualRegistration(settingsDirectory.Name + isolationLevel))));

            var integrationTransportProviders = new[]
            {
                (inMemoryIntegrationTransportIdentity, useInMemoryIntegrationTransport),
                (rabbitMqIntegrationTransportIdentity, useRabbitMqIntegrationTransport)
            };

            var dataAccessProviders = new Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder>[]
            {
                (builder, dataAccessOptions) => builder.WithPostgreSqlDataAccess(dataAccessOptions)
            };

            var eventSourcingProviders = new Func<IEndpointBuilder, IEndpointBuilder>[]
            {
                builder => builder.WithSqlEventSourcing()
            };

            var isolationLevels = new[]
            {
                IsolationLevel.Snapshot,
                IsolationLevel.ReadCommitted
            };

            return integrationTransportProviders
               .SelectMany(transport =>
               {
                   var (transportIdentity, useTransport) = transport;

                   return dataAccessProviders
                       .SelectMany(withDataAccess => eventSourcingProviders
                           .SelectMany(withEventSourcing => isolationLevels
                               .Select(isolationLevel => new object[]
                               {
                                   settingsDirectoryProducer,
                                   transportIdentity,
                                   useTransport(isolationLevel),
                                   withDataAccess,
                                   withEventSourcing,
                                   isolationLevel
                               })));
               });
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostEndpointDataAccessTestData))]
        internal async Task Migration_generates_database_model_changes(
            DirectoryInfo settingsDirectory,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            TimeSpan timeout)
        {
            var databaseEntities = new[]
            {
                typeof(DatabaseEntity),
                typeof(ComplexDatabaseEntity),
                typeof(Community),
                typeof(Participant),
                typeof(Blog),
                typeof(Post),
                typeof(User)
            };

            var startupActions = new[]
            {
                typeof(RecreatePostgreSqlDatabaseHostedServiceStartupAction)
            };

            var additionalOurTypes = databaseEntities
                .Concat(startupActions)
                .ToArray();

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport)
                .UseEndpoint(TestIdentity.Endpoint10,
                    builder => withEventSourcing(withDataAccess(builder, options => options
                            .ExecuteMigrations()))
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(additionalOurTypes)
                            .WithAdditionalOurTypes(typeof(RecreatePostgreSqlDatabaseHostedServiceStartupAction)))
                        .BuildOptions())
                .BuildHost(settingsDirectory);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var endpointContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                await endpointContainer
                    .Resolve<RecreatePostgreSqlDatabaseHostedServiceStartupAction>()
                    .Run(cts.Token)
                    .ConfigureAwait(false);

                var modelProvider = endpointContainer.Resolve<IModelProvider>();

                var actualModel = await endpointContainer.InvokeWithinTransaction(
                        false,
                        endpointContainer.Resolve<IDatabaseModelBuilder>().BuildModel,
                        cts.Token)
                    .ConfigureAwait(false);

                databaseEntities = endpointContainer
                   .Resolve<IDatabaseTypeProvider>()
                   .DatabaseEntities()
                   .ToArray();

                var expectedModel = await endpointContainer
                    .Resolve<ICodeModelBuilder>()
                    .BuildModel(databaseEntities, cts.Token)
                    .ConfigureAwait(false);

                var unorderedModelChanges = endpointContainer
                    .Resolve<IModelComparator>()
                    .ExtractDiff(actualModel, expectedModel);

                var modelChanges = endpointContainer
                    .Resolve<IModelChangesSorter>()
                    .Sort(unorderedModelChanges)
                    .ToArray();

                modelChanges.Each((change, i) => Output.WriteLine($"[{i}] {change}"));

                var databaseConnectionProvider = endpointContainer.Resolve<IDatabaseConnectionProvider>();

                if (databaseConnectionProvider.GetType() == typeof(DatabaseConnectionProvider))
                {
                    var assertions = new Action<int>[]
                    {
                        index => AssertCreateSchema(modelChanges, index, nameof(GenericEndpoint.DataAccess.Sql.Deduplication)),
                        index => AssertCreateSchema(modelChanges, index, nameof(GenericEndpoint.EventSourcing)),
                        index => AssertCreateSchema(modelChanges, index, nameof(GenericHost) + nameof(Test)),
                        index => AssertCreateSchema(modelChanges, index, nameof(DataAccess.Orm.Sql.Migrations)),
                        index => AssertCreateEnumType(modelChanges, index, nameof(GenericHost) + nameof(Test), nameof(EnEnum), nameof(EnEnum.One), nameof(EnEnum.Two), nameof(EnEnum.Three)),
                        index => AssertCreateEnumType(modelChanges, index, nameof(GenericHost) + nameof(Test), nameof(EnEnumFlags), nameof(EnEnumFlags.A), nameof(EnEnumFlags.B), nameof(EnEnumFlags.C)),
                        index => AssertCreateEnumType(modelChanges, index, nameof(DataAccess.Orm.Sql.Migrations), nameof(EnColumnConstraintType), nameof(EnColumnConstraintType.PrimaryKey), nameof(EnColumnConstraintType.ForeignKey)),
                        index => AssertCreateEnumType(modelChanges, index, nameof(DataAccess.Orm.Sql.Migrations), nameof(EnTriggerEvent), nameof(EnTriggerEvent.Insert), nameof(EnTriggerEvent.Update), nameof(EnTriggerEvent.Delete)),
                        index => AssertCreateEnumType(modelChanges, index, nameof(DataAccess.Orm.Sql.Migrations), nameof(EnTriggerType), nameof(EnTriggerType.Before), nameof(EnTriggerType.After)),
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericEndpoint.DataAccess.Sql.Deduplication),
                                typeof(GenericEndpoint.DataAccess.Sql.Deduplication.IntegrationMessage),
                                new[]
                                {
                                    (nameof(GenericEndpoint.DataAccess.Sql.Deduplication.IntegrationMessage.PrimaryKey), "not null primary key"),
                                    (nameof(GenericEndpoint.DataAccess.Sql.Deduplication.IntegrationMessage.Version), "not null"),
                                    (nameof(GenericEndpoint.DataAccess.Sql.Deduplication.IntegrationMessage.Payload), "not null"),
                                    (nameof(GenericEndpoint.DataAccess.Sql.Deduplication.IntegrationMessage.ReflectedType), "not null")
                                },
                                new[]
                                {
                                    nameof(GenericEndpoint.DataAccess.Sql.Deduplication.IntegrationMessage.Headers)
                                });
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericEndpoint.DataAccess.Sql.Deduplication),
                                typeof(IntegrationMessageHeader),
                                new[]
                                {
                                    (nameof(IntegrationMessageHeader.PrimaryKey), "not null primary key"),
                                    (nameof(IntegrationMessageHeader.Version), "not null"),
                                    (nameof(IntegrationMessageHeader.Message), $@"not null references ""{nameof(GenericEndpoint.DataAccess.Sql.Deduplication)}"".""{nameof(GenericEndpoint.DataAccess.Sql.Deduplication.IntegrationMessage)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade"),
                                    (nameof(IntegrationMessageHeader.Payload), "not null")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericDomain.EventSourcing),
                                typeof(DatabaseDomainEvent),
                                new[]
                                {
                                    (nameof(DatabaseDomainEvent.PrimaryKey), "not null primary key"),
                                    (nameof(DatabaseDomainEvent.Version), "not null"),
                                    (nameof(DatabaseDomainEvent.AggregateId), "not null"),
                                    (nameof(DatabaseDomainEvent.Index), "not null"),
                                    (nameof(DatabaseDomainEvent.Timestamp), "not null"),
                                    (nameof(DatabaseDomainEvent.DomainEvent), "not null")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericHost) + nameof(Test),
                                typeof(Blog),
                                new[]
                                {
                                    (nameof(Blog.PrimaryKey), "not null primary key"),
                                    (nameof(Blog.Version), "not null"),
                                    (nameof(Blog.Theme), "not null")
                                },
                                new[]
                                {
                                    nameof(Blog.Posts)
                                });
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericHost) + nameof(Test),
                                typeof(Community),
                                new[]
                                {
                                    (nameof(Community.PrimaryKey), "not null primary key"),
                                    (nameof(Community.Version), "not null"),
                                    (nameof(Community.Name), "not null")
                                },
                                new[]
                                {
                                    nameof(Community.Participants)
                                });
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericHost) + nameof(Test),
                                typeof(DatabaseEntity),
                                new[]
                                {
                                    (nameof(DatabaseEntity.PrimaryKey), "not null primary key"),
                                    (nameof(DatabaseEntity.Version), "not null"),
                                    (nameof(DatabaseEntity.BooleanField), "not null"),
                                    (nameof(DatabaseEntity.StringField), "not null"),
                                    (nameof(DatabaseEntity.NullableStringField), string.Empty),
                                    (nameof(DatabaseEntity.IntField), "not null"),
                                    (nameof(DatabaseEntity.Enum), "not null")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericHost) + nameof(Test),
                                typeof(Participant),
                                new[]
                                {
                                    (nameof(Participant.PrimaryKey), "not null primary key"),
                                    (nameof(Participant.Version), "not null"),
                                    (nameof(Participant.Name), "not null")
                                },
                                new[]
                                {
                                    nameof(Participant.Communities)
                                });
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericHost) + nameof(Test),
                                typeof(User),
                                new[]
                                {
                                    (nameof(User.PrimaryKey), "not null primary key"),
                                    (nameof(User.Version), "not null"),
                                    (nameof(User.Nickname), "not null")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(DataAccess.Orm.Sql.Migrations),
                                typeof(AppliedMigration),
                                new[]
                                {
                                    (nameof(AppliedMigration.PrimaryKey), "not null primary key"),
                                    (nameof(AppliedMigration.Version), "not null"),
                                    (nameof(AppliedMigration.DateTime), "not null"),
                                    (nameof(AppliedMigration.CommandText), "not null"),
                                    (nameof(AppliedMigration.Name), "not null")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(DataAccess.Orm.Sql.Migrations),
                                typeof(FunctionView),
                                new[]
                                {
                                    (nameof(FunctionView.PrimaryKey), "not null primary key"),
                                    (nameof(FunctionView.Version), "not null"),
                                    (nameof(FunctionView.Schema), "not null"),
                                    (nameof(FunctionView.Function), "not null"),
                                    (nameof(FunctionView.Definition), "not null")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(DataAccess.Orm.Sql.Migrations),
                                typeof(SqlView),
                                new[]
                                {
                                    (nameof(SqlView.PrimaryKey), "not null primary key"),
                                    (nameof(SqlView.Version), "not null"),
                                    (nameof(SqlView.Schema), "not null"),
                                    (nameof(SqlView.View), "not null"),
                                    (nameof(SqlView.Query), "not null")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericEndpoint.DataAccess.Sql.Deduplication),
                                typeof(InboxMessage),
                                new[]
                                {
                                    (nameof(InboxMessage.PrimaryKey), "not null primary key"),
                                    (nameof(InboxMessage.Version), "not null"),
                                    (nameof(InboxMessage.Message), $@"not null references ""{nameof(GenericEndpoint.DataAccess.Sql.Deduplication)}"".""{nameof(IntegrationMessage)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade"),
                                    (nameof(InboxMessage.EndpointLogicalName), "not null"),
                                    (nameof(InboxMessage.EndpointInstanceName), "not null"),
                                    (nameof(InboxMessage.IsError), "not null"),
                                    (nameof(InboxMessage.Handled), "not null")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateMtmTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericEndpoint.DataAccess.Sql.Deduplication),
                                $"{nameof(IntegrationMessage)}_{nameof(IntegrationMessageHeader)}",
                                new[]
                                {
                                    (nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), $@"not null references ""{nameof(GenericEndpoint.DataAccess.Sql.Deduplication)}"".""{nameof(IntegrationMessage)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade"),
                                    (nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), $@"not null references ""{nameof(GenericEndpoint.DataAccess.Sql.Deduplication)}"".""{nameof(IntegrationMessageHeader)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade")
                                });
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericEndpoint.DataAccess.Sql.Deduplication),
                                typeof(OutboxMessage),
                                new[]
                                {
                                    (nameof(OutboxMessage.PrimaryKey), "not null primary key"),
                                    (nameof(OutboxMessage.Version), "not null"),
                                    (nameof(OutboxMessage.OutboxId), "not null"),
                                    (nameof(OutboxMessage.Timestamp), "not null"),
                                    (nameof(OutboxMessage.EndpointLogicalName), "not null"),
                                    (nameof(OutboxMessage.EndpointInstanceName), "not null"),
                                    (nameof(OutboxMessage.Message), $@"not null references ""{nameof(GenericEndpoint.DataAccess.Sql.Deduplication)}"".""{nameof(IntegrationMessage)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade"),
                                    (nameof(OutboxMessage.Sent), "not null")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateMtmTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericHost) + nameof(Test),
                                $"{nameof(Community)}_{nameof(Participant)}",
                                new[]
                                {
                                    (nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Community)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade"),
                                    (nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Participant)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade")
                                });
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericHost) + nameof(Test),
                                typeof(ComplexDatabaseEntity),
                                new[]
                                {
                                    (nameof(ComplexDatabaseEntity.PrimaryKey), "not null primary key"),
                                    (nameof(ComplexDatabaseEntity.Version), "not null"),
                                    (nameof(ComplexDatabaseEntity.Number), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableNumber), string.Empty),
                                    (nameof(ComplexDatabaseEntity.Identifier), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableIdentifier), string.Empty),
                                    (nameof(ComplexDatabaseEntity.Boolean), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableBoolean), string.Empty),
                                    (nameof(ComplexDatabaseEntity.DateTime), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableDateTime), string.Empty),
                                    (nameof(ComplexDatabaseEntity.TimeSpan), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableTimeSpan), string.Empty),
                                    (nameof(ComplexDatabaseEntity.DateOnly), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableDateOnly), string.Empty),
                                    (nameof(ComplexDatabaseEntity.TimeOnly), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableTimeOnly), string.Empty),
                                    (nameof(ComplexDatabaseEntity.ByteArray), "not null"),
                                    (nameof(ComplexDatabaseEntity.String), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableString), string.Empty),
                                    (nameof(ComplexDatabaseEntity.Enum), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableEnum), string.Empty),
                                    (nameof(ComplexDatabaseEntity.EnumFlags), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableEnumFlags), string.Empty),
                                    (nameof(ComplexDatabaseEntity.EnumArray), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableEnumArray), "not null"),
                                    (nameof(ComplexDatabaseEntity.StringArray), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableStringArray), "not null"),
                                    (nameof(ComplexDatabaseEntity.DateTimeArray), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableDateTimeArray), "not null"),
                                    (nameof(ComplexDatabaseEntity.Json), "not null"),
                                    (nameof(ComplexDatabaseEntity.NullableJson), string.Empty),
                                    (nameof(ComplexDatabaseEntity.Relation), $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Blog)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete no action"),
                                    (nameof(ComplexDatabaseEntity.NullableRelation), $@"references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Blog)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete no action")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericHost) + nameof(Test),
                                typeof(Post),
                                new[]
                                {
                                    (nameof(Post.PrimaryKey), "not null primary key"),
                                    (nameof(Post.Version), "not null"),
                                    (nameof(Post.DateTime), "not null"),
                                    (nameof(Post.Text), "not null"),
                                    (nameof(Post.Blog), $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Blog)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade"),
                                    (nameof(Post.User), $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(User)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete restrict")
                                },
                                Array.Empty<string>());
                        },
                        index =>
                        {
                            AssertCreateMtmTable(
                                modelProvider,
                                modelChanges,
                                index,
                                nameof(GenericHost) + nameof(Test),
                                $"{nameof(Blog)}_{nameof(Post)}",
                                new[]
                                {
                                    (nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Blog)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade"),
                                    (nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), $@"not null references ""{nameof(GenericHost) + nameof(Test)}"".""{nameof(Post)}"" (""{nameof(IUniqueIdentified.PrimaryKey)}"") on delete cascade")
                                });
                        },
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseColumn)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseColumnConstraint)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseEnumType)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseFunction)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseIndexColumn)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseSchema)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseTrigger)),
                        index => AssertCreateView(modelChanges, index, nameof(DatabaseView)),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(GenericEndpoint.DataAccess.Sql.Deduplication), $"{nameof(IntegrationMessage)}_{nameof(IntegrationMessageHeader)}", new[] { nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(GenericEndpoint.EventSourcing), nameof(DatabaseDomainEvent), new[] { nameof(DatabaseDomainEvent.AggregateId), nameof(DatabaseDomainEvent.Index) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, false, null, nameof(GenericEndpoint.EventSourcing), nameof(DatabaseDomainEvent), new[] { nameof(DatabaseDomainEvent.DomainEvent) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(GenericHost) + nameof(Test), $"{nameof(Blog)}_{nameof(Post)}", new[] { nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(GenericHost) + nameof(Test), $"{nameof(Community)}_{nameof(Participant)}", new[] { nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, false, $@"""{nameof(DatabaseEntity.BooleanField)}""",  nameof(GenericHost) + nameof(Test), nameof(DatabaseEntity), new[] { nameof(DatabaseEntity.StringField) }, new[] { nameof(DatabaseEntity.IntField) }),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Migrations), nameof(AppliedMigration), new[] { nameof(AppliedMigration.Name) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Migrations), nameof(DatabaseColumn), new[] { nameof(DatabaseColumn.Column), nameof(DatabaseColumn.Schema), nameof(DatabaseColumn.Table) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Migrations), nameof(DatabaseEnumType), new[] { nameof(DatabaseView.Schema), nameof(DatabaseEnumType.Type), nameof(DatabaseEnumType.Value) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Migrations), nameof(DatabaseFunction), new[] { nameof(DatabaseFunction.Function), nameof(DatabaseFunction.Schema) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Migrations), nameof(DatabaseIndexColumn), new[] { nameof(DatabaseIndexColumn.Column), nameof(DatabaseIndexColumn.Index), nameof(DatabaseIndexColumn.Schema), nameof(DatabaseIndexColumn.Table) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Migrations), nameof(DatabaseSchema), new[] { nameof(DatabaseSchema.Name) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Migrations), nameof(DatabaseTrigger), new[] { nameof(DatabaseTrigger.Schema), nameof(DatabaseTrigger.Trigger) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Migrations), nameof(DatabaseView), new[] { nameof(DatabaseView.Schema), nameof(DatabaseView.View) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Migrations), nameof(FunctionView), new[] { nameof(FunctionView.Function), nameof(FunctionView.Schema) }, Array.Empty<string>()),
                        index => AssertCreateIndex(modelProvider, modelChanges, index, true, null, nameof(DataAccess.Orm.Sql.Migrations), nameof(SqlView), new[] { nameof(SqlView.Schema), nameof(SqlView.View) }, Array.Empty<string>()),
                        index => AssertCreateFunction(modelProvider, modelChanges, index, nameof(GenericEndpoint.EventSourcing), nameof(AppendOnlyAttribute)),
                        index => AssertCreateTrigger(modelProvider, modelChanges, index, nameof(GenericEndpoint.EventSourcing), $"{nameof(DatabaseDomainEvent)}_aotrg", nameof(AppendOnlyAttribute))
                    };

                    Assert.Equal(assertions.Length, modelChanges.Length);

                    for (var i = 0; i < assertions.Length; i++)
                    {
                        assertions[i](i);
                    }

                    static void AssertCreateTable(
                        IModelProvider modelProvider,
                        IModelChange[] modelChanges,
                        int index,
                        string schema,
                        Type table,
                        (string column, string constraints)[] columnsAssertions,
                        string[] mtmColumnsAssertions)
                    {
                        Assert.True(modelChanges[index] is CreateTable);
                        var createTable = (CreateTable)modelChanges[index];
                        Assert.Equal($"{schema}.{table.Name}", $"{createTable.Schema}.{createTable.Table}");

                        AssertColumns(modelProvider, modelChanges, index, columnsAssertions);
                        AssertMtmColumns(modelProvider, modelChanges, index, mtmColumnsAssertions);
                    }

                    static void AssertCreateMtmTable(
                        IModelProvider modelProvider,
                        IModelChange[] modelChanges,
                        int index,
                        string schema,
                        string table,
                        (string column, string constraints)[] columnsAssertions)
                    {
                        Assert.True(modelChanges[index] is CreateTable);
                        var createTable = (CreateTable)modelChanges[index];
                        Assert.Equal($"{schema}.{table}", $"{createTable.Schema}.{createTable.Table}");

                        AssertColumns(modelProvider, modelChanges, index, columnsAssertions);
                        AssertMtmColumns(modelProvider, modelChanges, index, Array.Empty<string>());
                    }

                    static void AssertColumns(
                        IModelProvider modelProvider,
                        IModelChange[] modelChanges,
                        int index,
                        (string column, string constraints)[] assertions)
                    {
                        Assert.True(modelChanges[index] is CreateTable);
                        var createTable = (CreateTable)modelChanges[index];
                        Assert.True(modelProvider.TablesMap.ContainsKey(createTable.Schema));
                        Assert.True(modelProvider.TablesMap[createTable.Schema].ContainsKey(createTable.Table));
                        Assert.True(modelProvider.TablesMap[createTable.Schema][createTable.Table] is TableInfo);
                        var tableInfo = (TableInfo)modelProvider.TablesMap[createTable.Schema][createTable.Table];
                        Assert.Equal(tableInfo.Columns.Values.Count(column => !column.IsMultipleRelation), assertions.Length);

                        foreach (var (column, constraints) in assertions)
                        {
                            Assert.True(tableInfo.Columns.ContainsKey(column));
                            var columnInfo = tableInfo.Columns[column];
                            var actualConstraints = columnInfo.Constraints.ToString(" ");
                            Assert.Equal(actualConstraints, constraints, ignoreCase: true);
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
                    }

                    static void AssertMtmColumns(
                        IModelProvider modelProvider,
                        IModelChange[] modelChanges,
                        int index,
                        string[] columns)
                    {
                        Assert.True(modelChanges[index] is CreateTable);
                        var createTable = (CreateTable)modelChanges[index];
                        Assert.True(modelProvider.TablesMap.ContainsKey(createTable.Schema));
                        Assert.True(modelProvider.TablesMap[createTable.Schema].ContainsKey(createTable.Table));
                        Assert.True(modelProvider.TablesMap[createTable.Schema][createTable.Table] is TableInfo);
                        var tableInfo = (TableInfo)modelProvider.TablesMap[createTable.Schema][createTable.Table];
                        Assert.Equal(tableInfo.Columns.Values.Count(column => column.IsMultipleRelation), columns.Length);

                        foreach (var column in columns)
                        {
                            Assert.True(tableInfo.Columns.ContainsKey(column));
                            var columnInfo = tableInfo.Columns[column];
                            Assert.True(columnInfo.IsMultipleRelation);
                            Assert.NotNull(columnInfo.Relation);
                        }
                    }
                }
                else
                {
                    throw new NotSupportedException(databaseConnectionProvider.GetType().FullName);
                }

                static void AssertCreateSchema(
                    IModelChange[] modelChanges,
                    int index,
                    string schema)
                {
                    Assert.True(modelChanges[index] is CreateSchema);
                    var createSchema = (CreateSchema)modelChanges[index];
                    Assert.Equal(createSchema.Schema, schema, ignoreCase: true);
                }

                static void AssertCreateEnumType(
                    IModelChange[] modelChanges,
                    int index,
                    string schema,
                    string type,
                    params string[] values)
                {
                    Assert.True(modelChanges[index] is CreateEnumType);
                    var createEnumType = (CreateEnumType)modelChanges[index];
                    Assert.Equal(createEnumType.Schema, schema, ignoreCase: true);
                    Assert.Equal(createEnumType.Type, type, ignoreCase: true);
                    Assert.True(createEnumType.Values.SequenceEqual(values, StringComparer.Ordinal));
                }

                static void AssertCreateView(
                    IModelChange[] modelChanges,
                    int index,
                    string view)
                {
                    Assert.True(modelChanges[index] is CreateView);
                    var createView = (CreateView)modelChanges[index];
                    Assert.Equal(createView.View, view, ignoreCase: true);
                }

                static void AssertCreateIndex(
                    IModelProvider modelProvider,
                    IModelChange[] modelChanges,
                    int index,
                    bool unique,
                    string? predicate,
                    string schema,
                    string table,
                    string[] columns,
                    string[] includedColumns)
                {
                    Assert.True(modelChanges[index] is CreateIndex);
                    var createIndex = (CreateIndex)modelChanges[index];
                    Assert.Equal(createIndex.Schema, schema, ignoreCase: true);
                    Assert.Equal(createIndex.Table, table, ignoreCase: true);
                    var indexName = (table, columns.ToString("_")).ToString("__");
                    Assert.Equal(createIndex.Index, indexName, ignoreCase: true);
                    var indexInfo = modelProvider.TablesMap[schema][table].Indexes[createIndex.Index];
                    Assert.True(includedColumns.OrderBy(column => column).SequenceEqual(indexInfo.IncludedColumns.Select(column => column.Name).OrderBy(column => column), StringComparer.OrdinalIgnoreCase));
                    Assert.Equal(unique, indexInfo.Unique);
                    Assert.Equal(predicate, indexInfo.Predicate);
                }

                static void AssertCreateFunction(
                    IModelProvider modelProvider,
                    IModelChange[] modelChanges,
                    int index,
                    string schema,
                    string function)
                {
                    Assert.True(modelChanges[index] is CreateFunction);
                    var createFunction = (CreateFunction)modelChanges[index];
                    Assert.Equal(createFunction.Schema, schema, ignoreCase: true);
                    Assert.Equal(createFunction.Function, function, ignoreCase: true);
                }

                static void AssertCreateTrigger(
                    IModelProvider modelProvider,
                    IModelChange[] modelChanges,
                    int index,
                    string schema,
                    string trigger,
                    string function)
                {
                    Assert.True(modelChanges[index] is CreateTrigger);
                    var createTrigger = (CreateTrigger)modelChanges[index];
                    Assert.Equal(createTrigger.Schema, schema, ignoreCase: true);
                    Assert.Equal(createTrigger.Trigger, trigger, ignoreCase: true);
                    Assert.Equal(createTrigger.Function, function, ignoreCase: true);
                }
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(BuildHostEndpointDataAccessTestData))]
        internal async Task Equivalent_database_models_have_no_model_changes(
            DirectoryInfo settingsDirectory,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            TimeSpan timeout)
        {
            var startupActions = new[]
            {
                typeof(RecreatePostgreSqlDatabaseHostedServiceStartupAction)
            };

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport)
                .UseEndpoint(TestIdentity.Endpoint10,
                    builder => withEventSourcing(withDataAccess(builder, options => options
                            .ExecuteMigrations()))
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(startupActions))
                        .BuildOptions())
                .BuildHost(settingsDirectory);

            using (host)
            using (var cts = new CancellationTokenSource(timeout))
            {
                var endpointContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                await endpointContainer
                    .Resolve<RecreatePostgreSqlDatabaseHostedServiceStartupAction>()
                    .Run(cts.Token)
                    .ConfigureAwait(false);

                var actualModel = await endpointContainer.InvokeWithinTransaction(
                        false,
                        endpointContainer.Resolve<IDatabaseModelBuilder>().BuildModel,
                        cts.Token)
                    .ConfigureAwait(false);

                var databaseEntities = endpointContainer
                    .Resolve<IDatabaseTypeProvider>()
                    .DatabaseEntities()
                    .ToArray();

                var expectedModel = await endpointContainer
                    .Resolve<ICodeModelBuilder>()
                    .BuildModel(databaseEntities, cts.Token)
                    .ConfigureAwait(false);

                var modelChanges = endpointContainer
                    .Resolve<IModelComparator>()
                    .ExtractDiff(actualModel, expectedModel);

                Assert.NotEmpty(modelChanges);

                modelChanges = endpointContainer
                    .Resolve<IModelComparator>()
                    .ExtractDiff(expectedModel, expectedModel);

                Assert.Empty(modelChanges);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(EndpointDataAccessTestData))]
        internal async Task Outbox_delivers_messages_in_background(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, DirectoryInfo, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            IsolationLevel isolationLevel)
        {
            var settingsDirectory = settingsDirectoryProducer("OutboxDeliversMessagesInBackground");

            var messageTypes = new[]
            {
                typeof(Request),
                typeof(Reply)
            };

            var messageHandlerTypes = new[]
            {
                typeof(AlwaysReplyRequestHandler),
                typeof(ReplyHandler)
            };

            var startupActions = new[]
            {
                typeof(RecreatePostgreSqlDatabaseHostedServiceStartupAction)
            };

            var additionalOurTypes = messageTypes
               .Concat(messageHandlerTypes)
               .Concat(startupActions)
               .ToArray();

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport, settingsDirectory)
                .UseEndpoint(TestIdentity.Endpoint10,
                    builder => withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(additionalOurTypes)
                            .WithManualRegistrations(new BackgroundOutboxDeliveryManualRegistration())
                            .WithManualRegistrations(new IsolationLevelManualRegistration(isolationLevel)))
                        .BuildOptions())
                .BuildHost(settingsDirectory);

            await host
                .RunTestHost(Output, TestCase, BackgroundOutboxDeliveryTestInternal(settingsDirectory, transportIdentity, isolationLevel))
                .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> BackgroundOutboxDeliveryTestInternal(
                DirectoryInfo settingsDirectory,
                TransportIdentity transportIdentity,
                IsolationLevel isolationLevel)
            {
                return async (_, host, token) =>
                {
                    var transportDependencyContainer = host.GetIntegrationTransportDependencyContainer(transportIdentity);
                    var endpointDependencyContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                    var sqlDatabaseSettings = endpointDependencyContainer
                        .Resolve<ISettingsProvider<SqlDatabaseSettings>>()
                        .Get();

                    Assert.Equal(settingsDirectory.Name, sqlDatabaseSettings.Database);
                    Assert.Equal(isolationLevel, sqlDatabaseSettings.IsolationLevel);
                    Assert.Equal(1u, sqlDatabaseSettings.ConnectionPoolSize);

                    if (transportDependencyContainer.Resolve<IIntegrationTransport>() is RabbitMqIntegrationTransport)
                    {
                        var rabbitMqSettings = transportDependencyContainer
                            .Resolve<ISettingsProvider<RabbitMqSettings>>()
                            .Get();

                        Assert.Equal(settingsDirectory.Name + isolationLevel, rabbitMqSettings.VirtualHost);
                    }

                    var outboxSettings = endpointDependencyContainer
                        .Resolve<ISettingsProvider<OutboxSettings>>()
                        .Get();

                    Assert.Equal(TimeSpan.FromSeconds(1), outboxSettings.OutboxDeliveryInterval);

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                        var awaiter = Task.WhenAll(
                            collector.WaitUntilMessageIsNotReceived<Request>(),
                            collector.WaitUntilMessageIsNotReceived<Reply>(),
                            collector.WaitUntilErrorMessageIsNotReceived<HandlerInvoked>(message => message.HandlerType == typeof(ReplyHandler) && message.EndpointIdentity == TestIdentity.Endpoint10));

                        var integrationMessage = endpointDependencyContainer
                            .Resolve<IIntegrationMessageFactory>()
                            .CreateGeneralMessage(
                                new Request(42),
                                typeof(Request),
                                Array.Empty<IIntegrationMessageHeader>(),
                                null);

                        await transportDependencyContainer
                            .Resolve<IIntegrationTransport>()
                            .Enqueue(integrationMessage, token)
                            .ConfigureAwait(false);

                        await awaiter.ConfigureAwait(false);
                    }
                };
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(EndpointDataAccessTestData))]
        internal async Task Endpoint_applies_optimistic_concurrency_control(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, DirectoryInfo, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            IsolationLevel isolationLevel)
        {
            var settingsDirectory = settingsDirectoryProducer("EndpointAppliesOptimisticConcurrencyControl");

            var databaseEntities = new[]
            {
                typeof(DatabaseEntity)
            };

            var startupActions = new[]
            {
                typeof(RecreatePostgreSqlDatabaseHostedServiceStartupAction)
            };

            var additionalOurTypes = databaseEntities
               .Concat(startupActions)
               .ToArray();

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport, settingsDirectory)
                .UseEndpoint(TestIdentity.Endpoint10,
                    builder => withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(additionalOurTypes)
                            .WithManualRegistrations(new IsolationLevelManualRegistration(isolationLevel)))
                        .BuildOptions())
                .BuildHost(settingsDirectory);

            await host
                .RunTestHost(Output, TestCase, OptimisticConcurrencyTestInternal(settingsDirectory, transportIdentity, isolationLevel))
                .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> OptimisticConcurrencyTestInternal(
                DirectoryInfo settingsDirectory,
                TransportIdentity transportIdentity,
                IsolationLevel isolationLevel)
            {
                return async (_, host, token) =>
                {
                    var transportDependencyContainer = host.GetIntegrationTransportDependencyContainer(transportIdentity);
                    var endpointDependencyContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                    var sqlDatabaseSettings = endpointDependencyContainer
                        .Resolve<ISettingsProvider<SqlDatabaseSettings>>()
                        .Get();

                    Assert.Equal(settingsDirectory.Name, sqlDatabaseSettings.Database);
                    Assert.Equal(isolationLevel, sqlDatabaseSettings.IsolationLevel);
                    Assert.Equal(3u, sqlDatabaseSettings.ConnectionPoolSize);

                    if (transportDependencyContainer.Resolve<IIntegrationTransport>() is RabbitMqIntegrationTransport)
                    {
                        var rabbitMqSettings = transportDependencyContainer
                            .Resolve<ISettingsProvider<RabbitMqSettings>>()
                            .Get();

                        Assert.Equal(settingsDirectory.Name + isolationLevel, rabbitMqSettings.VirtualHost);
                    }

                    var primaryKey = Guid.NewGuid();

                    // #1 - create/create
                    {
                        Exception? exception = null;

                        try
                        {
                            await Task.WhenAll(
                                    CreateEntity(endpointDependencyContainer, primaryKey, token),
                                    CreateEntity(endpointDependencyContainer, primaryKey, token))
                               .ConfigureAwait(false);
                        }
                        catch (DatabaseException databaseException) when (databaseException is not DatabaseConcurrentUpdateException)
                        {
                            exception = databaseException;
                        }

                        Assert.NotNull(exception);
                        Assert.True(exception is DatabaseException);
                        Assert.NotNull(exception.InnerException);
                        Assert.Contains(exception.Flatten(), ex => ex.IsUniqueViolation());

                        var entity = await ReadEntity(endpointDependencyContainer, primaryKey, token).ConfigureAwait(false);

                        Assert.NotNull(entity);
                        Assert.NotEqual(default, entity.Version);
                        Assert.Equal(42, entity.IntField);
                    }

                    // #2 - update/update
                    {
                        Exception? exception = null;

                        try
                        {
                            var sync = new AsyncManualResetEvent(false);

                            var updateTask1 = UpdateEntity(endpointDependencyContainer, primaryKey, sync, token);
                            var updateTask2 = UpdateEntity(endpointDependencyContainer, primaryKey, sync, token);
                            var syncTask = Task.Delay(TimeSpan.FromMilliseconds(100), token).ContinueWith(_ => sync.Set(), token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

                            await Task.WhenAll(updateTask1, updateTask2, syncTask).ConfigureAwait(false);
                        }
                        catch (DatabaseConcurrentUpdateException concurrentUpdateException)
                        {
                            exception = concurrentUpdateException;
                        }

                        Assert.NotNull(exception);

                        var entity = await ReadEntity(endpointDependencyContainer, primaryKey, token).ConfigureAwait(false);

                        Assert.NotNull(entity);
                        Assert.NotEqual(default, entity.Version);
                        Assert.Equal(43, entity.IntField);
                    }

                    // #3 - update/delete
                    {
                        var sync = new AsyncManualResetEvent(false);

                        var updateTask = UpdateEntity(endpointDependencyContainer, primaryKey, sync, token);
                        var deleteTask = DeleteEntity(endpointDependencyContainer, primaryKey, sync, token);
                        var syncTask = Task.Delay(TimeSpan.FromMilliseconds(100), token).ContinueWith(_ => sync.Set(), token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

                        Exception? exception = null;

                        try
                        {
                            await Task.WhenAll(updateTask, deleteTask, syncTask).ConfigureAwait(false);
                        }
                        catch (DatabaseConcurrentUpdateException concurrentUpdateException)
                        {
                            exception = concurrentUpdateException;
                        }

                        Assert.NotNull(exception);

                        var entity = await ReadEntity(endpointDependencyContainer, primaryKey, token).ConfigureAwait(false);

                        if (updateTask.IsFaulted || deleteTask.IsCompletedSuccessfully)
                        {
                            Assert.Null(entity);
                        }
                        else
                        {
                            Assert.NotNull(entity);
                            Assert.NotEqual(default, entity.Version);
                            Assert.Equal(44, entity.IntField);
                        }
                    }
                };
            }

            static async Task CreateEntity(
                IDependencyContainer dependencyContainer,
                Guid primaryKey,
                CancellationToken token)
            {
                await using (dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var transaction = dependencyContainer.Resolve<IDatabaseTransaction>();

                    long version;

                    await using (await transaction.OpenScope(true, token).ConfigureAwait(false))
                    {
                        var entity = DatabaseEntity.Generate(primaryKey);

                        _ = await transaction
                           .Insert(new[] { entity }, EnInsertBehavior.Default)
                           .CachedExpression("4FC0EDC1-C658-4CA9-88A0-96DFA933AD7F")
                           .Invoke(token)
                           .ConfigureAwait(false);

                        version = entity.Version;
                    }

                    dependencyContainer
                       .Resolve<ILogger>()
                       .Debug($"{nameof(CreateEntity)}: {primaryKey} {version}");
                }
            }

            static async Task<DatabaseEntity?> ReadEntity(
                IDependencyContainer dependencyContainer,
                Guid primaryKey,
                CancellationToken token)
            {
                await using (dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var transaction = dependencyContainer.Resolve<IDatabaseTransaction>();

                    await using (await transaction.OpenScope(true, token).ConfigureAwait(false))
                    {
                        return await transaction
                           .SingleOrDefault<DatabaseEntity, Guid>(primaryKey, token)
                           .ConfigureAwait(false);
                    }
                }
            }

            static async Task UpdateEntity(
                IDependencyContainer dependencyContainer,
                Guid primaryKey,
                AsyncManualResetEvent sync,
                CancellationToken token)
            {
                await using (dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var transaction = dependencyContainer.Resolve<IDatabaseTransaction>();

                    await using (await transaction.OpenScope(true, token).ConfigureAwait(false))
                    {
                        _ = await transaction
                           .Update<DatabaseEntity>()
                           .Set(entity => entity.IntField.Assign(entity.IntField + 1))
                           .Where(entity => entity.PrimaryKey == primaryKey)
                           .CachedExpression("2CD18D09-05F8-4D80-AC19-96F7F524D28B")
                           .Invoke(token)
                           .ConfigureAwait(false);

                        await sync
                           .WaitAsync(token)
                           .ConfigureAwait(false);
                    }

                    dependencyContainer
                       .Resolve<ILogger>()
                       .Debug($"{nameof(UpdateEntity)}: {primaryKey}");
                }
            }

            static async Task DeleteEntity(
                IDependencyContainer dependencyContainer,
                Guid primaryKey,
                AsyncManualResetEvent sync,
                CancellationToken token)
            {
                await using (dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                {
                    var transaction = dependencyContainer.Resolve<IDatabaseTransaction>();

                    await using (await transaction.OpenScope(true, token).ConfigureAwait(false))
                    {
                        _ = await transaction
                           .Delete<DatabaseEntity>()
                           .Where(entity => entity.PrimaryKey == primaryKey)
                           .CachedExpression("2ADC594A-13EF-4F52-BD50-0F7FF62E5462")
                           .Invoke(token)
                           .ConfigureAwait(false);

                        await sync
                           .WaitAsync(token)
                           .ConfigureAwait(false);
                    }

                    dependencyContainer
                       .Resolve<ILogger>()
                       .Debug($"{nameof(DeleteEntity)}: {primaryKey}");
                }
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(EndpointDataAccessTestData))]
        internal async Task Orm_tracks_entity_changes(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, DirectoryInfo, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            IsolationLevel isolationLevel)
        {
            var settingsDirectory = settingsDirectoryProducer("OrmTracksEntityChanges");

            var databaseEntities = new[]
            {
                typeof(DatabaseEntity)
            };

            var startupActions = new[]
            {
                typeof(RecreatePostgreSqlDatabaseHostedServiceStartupAction)
            };

            var additionalOurTypes = databaseEntities
               .Concat(startupActions)
               .ToArray();

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport, settingsDirectory)
                .UseEndpoint(TestIdentity.Endpoint10,
                    builder => withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(additionalOurTypes)
                            .WithManualRegistrations(new IsolationLevelManualRegistration(isolationLevel)))
                        .BuildOptions())
                .BuildHost(settingsDirectory);

            await host
                .RunTestHost(Output, TestCase, ReactiveTransactionalStoreTestInternal(settingsDirectory, transportIdentity, isolationLevel))
                .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> ReactiveTransactionalStoreTestInternal(
                DirectoryInfo settingsDirectory,
                TransportIdentity transportIdentity,
                IsolationLevel isolationLevel)
            {
                return async (_, host, token) =>
                {
                    var transportDependencyContainer = host.GetIntegrationTransportDependencyContainer(transportIdentity);
                    var endpointDependencyContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                    var sqlDatabaseSettings = endpointDependencyContainer
                        .Resolve<ISettingsProvider<SqlDatabaseSettings>>()
                        .Get();

                    Assert.Equal(settingsDirectory.Name, sqlDatabaseSettings.Database);
                    Assert.Equal(isolationLevel, sqlDatabaseSettings.IsolationLevel);
                    Assert.Equal(1u, sqlDatabaseSettings.ConnectionPoolSize);

                    if (transportDependencyContainer.Resolve<IIntegrationTransport>() is RabbitMqIntegrationTransport)
                    {
                        var rabbitMqSettings = transportDependencyContainer
                            .Resolve<ISettingsProvider<RabbitMqSettings>>()
                            .Get();

                        Assert.Equal(settingsDirectory.Name + isolationLevel, rabbitMqSettings.VirtualHost);
                    }

                    // [I] - update transactional store without explicit reads
                    await using (endpointDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var transaction = endpointDependencyContainer.Resolve<IDatabaseTransaction>();
                        var transactionalStore = endpointDependencyContainer.Resolve<ITransactionalStore>();

                        await using (await transaction.OpenScope(true, token).ConfigureAwait(false))
                        {
                            var primaryKey = Guid.NewGuid();

                            // #0 - zero checks
                            Assert.False(transactionalStore.TryGetValue(primaryKey, out DatabaseEntity? storedEntry));
                            Assert.Null(storedEntry);

                            Assert.Null(await ReadEntity(transaction, primaryKey, token).ConfigureAwait(false));

                            // #1 - create
                            await CreateEntity(transaction, primaryKey, token).ConfigureAwait(false);

                            Assert.True(transactionalStore.TryGetValue(primaryKey, out storedEntry));

                            Assert.NotNull(storedEntry);
                            Assert.NotEqual(default, storedEntry.Version);
                            Assert.Equal(primaryKey, storedEntry.PrimaryKey);
                            Assert.Equal(42, storedEntry.IntField);

                            // #2 - update
                            await UpdateEntity(transaction, primaryKey, token).ConfigureAwait(false);

                            Assert.True(transactionalStore.TryGetValue(primaryKey, out storedEntry));

                            Assert.NotNull(storedEntry);
                            Assert.NotEqual(default, storedEntry.Version);
                            Assert.Equal(primaryKey, storedEntry.PrimaryKey);
                            Assert.Equal(43, storedEntry.IntField);

                            // #3 - delete
                            await DeleteEntity(transaction, primaryKey, token).ConfigureAwait(false);

                            Assert.False(transactionalStore.TryGetValue(primaryKey, out storedEntry));
                            Assert.Null(storedEntry);

                            Assert.Null(await ReadEntity(transaction, primaryKey, token).ConfigureAwait(false));
                        }
                    }

                    // [II] - update transactional store through explicit reads
                    await using (endpointDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var transaction = endpointDependencyContainer.Resolve<IDatabaseTransaction>();
                        var transactionalStore = endpointDependencyContainer.Resolve<ITransactionalStore>();

                        await using (await transaction.OpenScope(true, token).ConfigureAwait(false))
                        {
                            var primaryKey = Guid.NewGuid();

                            // #0 - zero checks
                            Assert.Null(await ReadEntity(transaction, primaryKey, token).ConfigureAwait(false));

                            Assert.False(transactionalStore.TryGetValue(primaryKey, out DatabaseEntity? storedEntry));
                            Assert.Null(storedEntry);

                            // #1 - create
                            await CreateEntity(transaction, primaryKey, token).ConfigureAwait(false);

                            var entity = await ReadEntity(transaction, primaryKey, token).ConfigureAwait(false);

                            Assert.NotNull(entity);
                            Assert.NotEqual(default, entity.Version);
                            Assert.Equal(primaryKey, entity.PrimaryKey);
                            Assert.Equal(42, entity.IntField);

                            Assert.True(transactionalStore.TryGetValue(primaryKey, out storedEntry));

                            Assert.NotNull(storedEntry);
                            Assert.NotEqual(default, storedEntry.Version);
                            Assert.Equal(primaryKey, storedEntry.PrimaryKey);
                            Assert.Equal(42, storedEntry.IntField);

                            Assert.Same(entity, storedEntry);

                            // #2 - update
                            await UpdateEntity(transaction, primaryKey, token).ConfigureAwait(false);

                            entity = await ReadEntity(transaction, primaryKey, token).ConfigureAwait(false);

                            Assert.NotNull(entity);
                            Assert.NotEqual(default, entity.Version);
                            Assert.Equal(primaryKey, entity.PrimaryKey);
                            Assert.Equal(43, entity.IntField);

                            Assert.True(transactionalStore.TryGetValue(primaryKey, out storedEntry));

                            Assert.NotNull(storedEntry);
                            Assert.NotEqual(default, storedEntry.Version);
                            Assert.Equal(primaryKey, storedEntry.PrimaryKey);
                            Assert.Equal(43, storedEntry.IntField);

                            Assert.Same(entity, storedEntry);

                            // #3 - delete
                            await DeleteEntity(transaction, primaryKey, token).ConfigureAwait(false);

                            Assert.Null(await ReadEntity(transaction, primaryKey, token).ConfigureAwait(false));

                            Assert.False(transactionalStore.TryGetValue(primaryKey, out storedEntry));
                            Assert.Null(storedEntry);
                        }
                    }

                    // [III] - keep reactive reference
                    await using (endpointDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var transaction = endpointDependencyContainer.Resolve<IDatabaseTransaction>();
                        var transactionalStore = endpointDependencyContainer.Resolve<ITransactionalStore>();

                        await using (await transaction.OpenScope(true, token).ConfigureAwait(false))
                        {
                            var primaryKey = Guid.NewGuid();

                            // #0 - zero checks
                            Assert.Null(await ReadEntity(transaction, primaryKey, token).ConfigureAwait(false));

                            Assert.False(transactionalStore.TryGetValue(primaryKey, out DatabaseEntity? storedEntry));
                            Assert.Null(storedEntry);

                            // #1 - create
                            await CreateEntity(transaction, primaryKey, token).ConfigureAwait(false);

                            var entity = await ReadEntity(transaction, primaryKey, token).ConfigureAwait(false);

                            Assert.NotNull(entity);
                            Assert.NotEqual(default, entity.Version);
                            Assert.Equal(primaryKey, entity.PrimaryKey);
                            Assert.Equal(42, entity.IntField);

                            // #2 - update
                            await UpdateEntity(transaction, primaryKey, token).ConfigureAwait(false);

                            Assert.NotNull(entity);
                            Assert.NotEqual(default, entity.Version);
                            Assert.Equal(primaryKey, entity.PrimaryKey);
                            Assert.Equal(43, entity.IntField);

                            // #3 - delete
                            await DeleteEntity(transaction, primaryKey, token).ConfigureAwait(false);

                            Assert.Null(await ReadEntity(transaction, primaryKey, token).ConfigureAwait(false));

                            Assert.False(transactionalStore.TryGetValue(primaryKey, out storedEntry));
                            Assert.Null(storedEntry);
                        }
                    }
                };
            }

            static async Task CreateEntity(
                IDatabaseTransaction transaction,
                Guid primaryKey,
                CancellationToken token)
            {
                var entity = DatabaseEntity.Generate(primaryKey);

                _ = await transaction
                   .Insert(new[] { entity }, EnInsertBehavior.Default)
                   .CachedExpression("19ECB7D0-20CC-4AB1-819E-B40E6AC56E98")
                   .Invoke(token)
                   .ConfigureAwait(false);
            }

            static async Task<DatabaseEntity?> ReadEntity(
                IDatabaseTransaction transaction,
                Guid primaryKey,
                CancellationToken token)
            {
                return await transaction
                   .SingleOrDefault<DatabaseEntity, Guid>(primaryKey, token)
                   .ConfigureAwait(false);
            }

            static async Task UpdateEntity(
                IDatabaseTransaction transaction,
                Guid primaryKey,
                CancellationToken token)
            {
                _ = await transaction
                   .Update<DatabaseEntity>()
                   .Set(entity => entity.IntField.Assign(entity.IntField + 1))
                   .Where(entity => entity.PrimaryKey == primaryKey)
                   .CachedExpression("8391A774-81D3-40C7-980D-C5F9A874C4B1")
                   .Invoke(token)
                   .ConfigureAwait(false);
            }

            static async Task DeleteEntity(
                IDatabaseTransaction transaction,
                Guid primaryKey,
                CancellationToken token)
            {
                _ = await transaction
                   .Delete<DatabaseEntity>()
                   .Where(entity => entity.PrimaryKey == primaryKey)
                   .CachedExpression("32A01EBD-B109-434F-BC0D-5EDEB2341289")
                   .Invoke(token)
                   .ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(EndpointDataAccessTestData))]
        internal async Task Only_commands_can_introduce_changes(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, DirectoryInfo, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            IsolationLevel isolationLevel)
        {
            var settingsDirectory = settingsDirectoryProducer("OnlyCommandsCanIntroduceChanges");

            var messageTypes = new[]
            {
                typeof(Command),
                typeof(Request),
                typeof(Reply),
                typeof(Event)
            };

            var messageHandlerTypes = new[]
            {
                typeof(IntroduceDatabaseChangesCommandHandler),
                typeof(IntroduceDatabaseChangesRequestHandler),
                typeof(IntroduceDatabaseChangesReplyHandler),
                typeof(IntroduceDatabaseChangesEventHandler)
            };

            var databaseEntities = new[]
            {
                typeof(DatabaseEntity)
            };

            var startupActions = new[]
            {
                typeof(RecreatePostgreSqlDatabaseHostedServiceStartupAction)
            };

            var additionalOurTypes = messageTypes
               .Concat(messageHandlerTypes)
               .Concat(databaseEntities)
               .Concat(startupActions)
               .ToArray();

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport, settingsDirectory)
                .UseEndpoint(TestIdentity.Endpoint10,
                    builder => withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(additionalOurTypes)
                            .WithManualRegistrations(new IsolationLevelManualRegistration(isolationLevel)))
                        .BuildOptions())
                .BuildHost(settingsDirectory);

            await host
                .RunTestHost(Output, TestCase, OnlyCommandsCanIntroduceChangesInternal(settingsDirectory, transportIdentity, isolationLevel))
                .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> OnlyCommandsCanIntroduceChangesInternal(
                DirectoryInfo settingsDirectory,
                TransportIdentity transportIdentity,
                IsolationLevel isolationLevel)
            {
                return async (output, host, token) =>
                {
                    var transportDependencyContainer = host.GetIntegrationTransportDependencyContainer(transportIdentity);
                    var endpointDependencyContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                    var sqlDatabaseSettings = endpointDependencyContainer
                        .Resolve<ISettingsProvider<SqlDatabaseSettings>>()
                        .Get();

                    Assert.Equal(settingsDirectory.Name, sqlDatabaseSettings.Database);
                    Assert.Equal(isolationLevel, sqlDatabaseSettings.IsolationLevel);
                    Assert.Equal(1u, sqlDatabaseSettings.ConnectionPoolSize);

                    if (transportDependencyContainer.Resolve<IIntegrationTransport>() is RabbitMqIntegrationTransport)
                    {
                        var rabbitMqSettings = transportDependencyContainer
                            .Resolve<ISettingsProvider<RabbitMqSettings>>()
                            .Get();

                        Assert.Equal(settingsDirectory.Name + isolationLevel, rabbitMqSettings.VirtualHost);
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                        var awaiter = collector.WaitUntilMessageIsNotReceived<Command>();

                        var integrationMessage = endpointDependencyContainer
                            .Resolve<IIntegrationMessageFactory>()
                            .CreateGeneralMessage(
                                new Command(42),
                                typeof(Command),
                                Array.Empty<IIntegrationMessageHeader>(),
                                null);

                        await transportDependencyContainer
                            .Resolve<IIntegrationTransport>()
                            .Enqueue(integrationMessage, token)
                            .ConfigureAwait(false);

                        await awaiter.ConfigureAwait(false);
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                        var awaiter = collector.WaitUntilErrorMessageIsNotReceived<Request>(
                            exceptionPredicate: exception =>
                                exception is InvalidOperationException &&
                                exception.Message.Contains("only commands can introduce changes", StringComparison.OrdinalIgnoreCase));

                        var integrationMessage = endpointDependencyContainer
                            .Resolve<IIntegrationMessageFactory>()
                            .CreateGeneralMessage(
                                new Request(42),
                                typeof(Request),
                                Array.Empty<IIntegrationMessageHeader>(),
                                null);

                        await transportDependencyContainer
                            .Resolve<IIntegrationTransport>()
                            .Enqueue(integrationMessage, token)
                            .ConfigureAwait(false);

                        await awaiter.ConfigureAwait(false);
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                        var awaiter = collector.WaitUntilErrorMessageIsNotReceived<Reply>(
                            exceptionPredicate: exception =>
                                exception is InvalidOperationException
                                && exception.Message.Contains("only commands can introduce changes", StringComparison.OrdinalIgnoreCase));

                        var request = new Request(42);

                        var initiatorMessage = endpointDependencyContainer
                            .Resolve<IIntegrationMessageFactory>()
                            .CreateGeneralMessage(
                                request,
                                typeof(Request),
                                Array.Empty<IIntegrationMessageHeader>(),
                                null);

                        var integrationMessage = endpointDependencyContainer
                            .Resolve<IIntegrationMessageFactory>()
                            .CreateGeneralMessage(
                                new Reply(request.Id),
                                typeof(Reply),
                                Array.Empty<IIntegrationMessageHeader>(),
                                initiatorMessage);

                        await transportDependencyContainer
                            .Resolve<IIntegrationTransport>()
                            .Enqueue(integrationMessage, token)
                            .ConfigureAwait(false);

                        await awaiter.ConfigureAwait(false);
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                        var awaiter = collector.WaitUntilErrorMessageIsNotReceived<Event>(
                            exceptionPredicate: exception =>
                                exception is InvalidOperationException
                                && exception.Message.Contains("only commands can introduce changes", StringComparison.OrdinalIgnoreCase));

                        var integrationMessage = endpointDependencyContainer
                            .Resolve<IIntegrationMessageFactory>()
                            .CreateGeneralMessage(
                                new Event(42),
                                typeof(Event),
                                Array.Empty<IIntegrationMessageHeader>(),
                                null);

                        await transportDependencyContainer
                            .Resolve<IIntegrationTransport>()
                            .Enqueue(integrationMessage, token)
                            .ConfigureAwait(false);

                        await awaiter.ConfigureAwait(false);
                    }
                };
            }
        }

        // TODO: #205 - recode after rpc-transport implementation
        [SuppressMessage("Analysis", "xUnit1004", Justification = "#205")]
        [Theory(Skip = "#205", Timeout = 60_000)]
        [MemberData(nameof(EndpointDataAccessTestData))]
        internal async Task Messaging_requires_authorization(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, DirectoryInfo, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            IsolationLevel isolationLevel)
        {
            var settingsDirectory = settingsDirectoryProducer("MessagingRequiresAuthorization");

            var startupActions = new[]
            {
                typeof(RecreatePostgreSqlDatabaseHostedServiceStartupAction)
            };

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport, settingsDirectory)
                .UseAuthEndpoint(builder =>
                    withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
                        .WithJwtAuthentication(builder.Context.Configuration)
                        .WithAuthorization()
                        .WithWebAuthorization()
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(startupActions)
                            .WithManualRegistrations(new IsolationLevelManualRegistration(isolationLevel)))
                        .BuildOptions())
                .BuildHost(settingsDirectory);

            await host
                .RunTestHost(Output, TestCase, AuthorizeUserTestInternal(settingsDirectory, transportIdentity, isolationLevel))
                .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> AuthorizeUserTestInternal(
                DirectoryInfo settingsDirectory,
                TransportIdentity transportIdentity,
                IsolationLevel isolationLevel)
            {
                return async (output, host, token) =>
                {
                    var transportDependencyContainer = host.GetIntegrationTransportDependencyContainer(transportIdentity);
                    var endpointDependencyContainer = host.GetEndpointDependencyContainer(AuthEndpoint.Contract.Identity.EndpointIdentity);

                    var sqlDatabaseSettings = endpointDependencyContainer
                        .Resolve<ISettingsProvider<SqlDatabaseSettings>>()
                        .Get();

                    Assert.Equal(settingsDirectory.Name, sqlDatabaseSettings.Database);
                    Assert.Equal(isolationLevel, sqlDatabaseSettings.IsolationLevel);
                    Assert.Equal(1u, sqlDatabaseSettings.ConnectionPoolSize);

                    if (transportDependencyContainer.Resolve<IIntegrationTransport>() is RabbitMqIntegrationTransport)
                    {
                        var rabbitMqSettings = transportDependencyContainer
                            .Resolve<ISettingsProvider<RabbitMqSettings>>()
                            .Get();

                        Assert.Equal(settingsDirectory.Name + isolationLevel, rabbitMqSettings.VirtualHost);
                    }

                    var username = "qwerty";
                    var password = "12345678";

                    var authorizationToken = endpointDependencyContainer
                        .Resolve<ITokenProvider>()
                        .GenerateToken(username, new[] { Features.Authentication }, TimeSpan.FromSeconds(60));

                    var initiatorMessage = endpointDependencyContainer
                        .Resolve<IIntegrationMessageFactory>()
                        .CreateGeneralMessage(
                            new Command(42),
                            typeof(Command),
                            new[] { new Authorization(authorizationToken) },
                            null);

                    var request = new AuthenticateUser(username, password);
                    UserAuthenticationResult? userAuthenticationResult;

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        userAuthenticationResult = await endpointDependencyContainer
                            .Resolve<IIntegrationContext>()
                            .RpcRequest<AuthenticateUser, UserAuthenticationResult>(request, CancellationToken.None)
                            .ConfigureAwait(false);
                    }

                    output.WriteLine(userAuthenticationResult.Dump(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty));

                    Assert.Equal(username, userAuthenticationResult.Username);
                    Assert.Empty(userAuthenticationResult.Token);

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                        var awaiter = Task.WhenAll(
                            collector.WaitUntilMessageIsNotReceived<CreateUser>(),
                            collector.WaitUntilMessageIsNotReceived<UserWasCreated>());

                        var integrationMessage = endpointDependencyContainer
                            .Resolve<IIntegrationMessageFactory>()
                            .CreateGeneralMessage(
                                new CreateUser(username, password),
                                typeof(CreateUser),
                                Array.Empty<IIntegrationMessageHeader>(),
                                null);

                        await transportDependencyContainer
                            .Resolve<IIntegrationTransport>()
                            .Enqueue(integrationMessage, token)
                            .ConfigureAwait(false);

                        await awaiter.ConfigureAwait(false);
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        userAuthenticationResult = await endpointDependencyContainer
                            .Resolve<IIntegrationContext>()
                            .RpcRequest<AuthenticateUser, UserAuthenticationResult>(request, CancellationToken.None)
                            .ConfigureAwait(false);
                    }

                    output.WriteLine(userAuthenticationResult.Dump(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty));

                    Assert.Equal(username, userAuthenticationResult.Username);
                    Assert.NotEmpty(userAuthenticationResult.Token);
                };
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(EndpointDataAccessTestData))]
        internal async Task Orm_applies_cascade_delete_strategy(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, DirectoryInfo, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            IsolationLevel isolationLevel)
        {
            var settingsDirectory = settingsDirectoryProducer("OrmAppliesCascadeDeleteStrategy");

            var messageTypes = new[]
            {
                typeof(Request),
                typeof(Reply)
            };

            var messageHandlerTypes = new[]
            {
                typeof(AlwaysReplyRequestHandler),
                typeof(ReplyHandler)
            };

            var startupActions = new[]
            {
                typeof(RecreatePostgreSqlDatabaseHostedServiceStartupAction)
            };

            var additionalOurTypes = messageTypes
                .Concat(messageHandlerTypes)
                .Concat(startupActions)
                .ToArray();

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport, settingsDirectory)
                .UseEndpoint(TestIdentity.Endpoint10,
                    builder => withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
                        .ModifyContainerOptions(options => options
                            .WithAdditionalOurTypes(additionalOurTypes)
                            .WithManualRegistrations(new IsolationLevelManualRegistration(isolationLevel)))
                        .BuildOptions())
                .BuildHost(settingsDirectory);

            await host
                .RunTestHost(Output, TestCase, CascadeDeleteTestInternal(settingsDirectory, transportIdentity, isolationLevel))
                .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> CascadeDeleteTestInternal(
                DirectoryInfo settingsDirectory,
                TransportIdentity transportIdentity,
                IsolationLevel isolationLevel)
            {
                return async (output, host, token) =>
                {
                    var transportDependencyContainer = host.GetIntegrationTransportDependencyContainer(transportIdentity);
                    var endpointDependencyContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                    var sqlDatabaseSettings = endpointDependencyContainer
                        .Resolve<ISettingsProvider<SqlDatabaseSettings>>()
                        .Get();

                    Assert.Equal(settingsDirectory.Name, sqlDatabaseSettings.Database);
                    Assert.Equal(isolationLevel, sqlDatabaseSettings.IsolationLevel);
                    Assert.Equal(1u, sqlDatabaseSettings.ConnectionPoolSize);

                    if (transportDependencyContainer.Resolve<IIntegrationTransport>() is RabbitMqIntegrationTransport)
                    {
                        var rabbitMqSettings = transportDependencyContainer
                            .Resolve<ISettingsProvider<RabbitMqSettings>>()
                            .Get();

                        Assert.Equal(settingsDirectory.Name + isolationLevel, rabbitMqSettings.VirtualHost);
                    }

                    var modelProvider = endpointDependencyContainer.Resolve<IModelProvider>();

                    var databaseEntities = endpointDependencyContainer
                        .Resolve<IDatabaseTypeProvider>()
                        .DatabaseEntities()
                        .ToList();

                    Assert.Contains(typeof(InboxMessage), databaseEntities);
                    Assert.Contains(typeof(OutboxMessage), databaseEntities);
                    Assert.Contains(typeof(IntegrationMessage), databaseEntities);
                    Assert.Contains(typeof(IntegrationMessageHeader), databaseEntities);

                    Assert.Equal(EnOnDeleteBehavior.Cascade, typeof(InboxMessage).GetProperty(nameof(InboxMessage.Message), BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)?.GetRequiredAttribute<ForeignKeyAttribute>().OnDeleteBehavior);
                    Assert.Equal(EnOnDeleteBehavior.Cascade, typeof(OutboxMessage).GetProperty(nameof(OutboxMessage.Message), BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)?.GetRequiredAttribute<ForeignKeyAttribute>().OnDeleteBehavior);
                    Assert.Equal(EnOnDeleteBehavior.Cascade, typeof(IntegrationMessageHeader).GetProperty(nameof(IntegrationMessageHeader.Message), BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)?.GetRequiredAttribute<ForeignKeyAttribute>().OnDeleteBehavior);

                    var mtmType = modelProvider
                        .TablesMap[nameof(GenericEndpoint.DataAccess.Sql.Deduplication)][$"{nameof(IntegrationMessage)}_{nameof(IntegrationMessageHeader)}"]
                        .Type;

                    Assert.Equal(EnOnDeleteBehavior.Cascade, mtmType.GetProperty(nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)?.GetRequiredAttribute<ForeignKeyAttribute>().OnDeleteBehavior);
                    Assert.Equal(EnOnDeleteBehavior.Cascade, mtmType.GetProperty(nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)?.GetRequiredAttribute<ForeignKeyAttribute>().OnDeleteBehavior);

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                        var awaiter = Task.WhenAll(
                            collector.WaitUntilMessageIsNotReceived<Request>(),
                            collector.WaitUntilMessageIsNotReceived<Reply>(),
                            collector.WaitUntilErrorMessageIsNotReceived<HandlerInvoked>(message => message.HandlerType == typeof(ReplyHandler) && message.EndpointIdentity == TestIdentity.Endpoint10));

                        var integrationMessage = endpointDependencyContainer
                            .Resolve<IIntegrationMessageFactory>()
                            .CreateGeneralMessage(
                                new Request(42),
                                typeof(Request),
                                Array.Empty<IIntegrationMessageHeader>(),
                                null);

                        await transportDependencyContainer
                            .Resolve<IIntegrationTransport>()
                            .Enqueue(integrationMessage, token)
                            .ConfigureAwait(false);

                        await awaiter.ConfigureAwait(false);
                    }

                    await endpointDependencyContainer
                        .InvokeWithinTransaction(false, modelProvider, CheckRows, token)
                        .ConfigureAwait(false);

                    await endpointDependencyContainer
                        .InvokeWithinTransaction(true, Delete, token)
                        .ConfigureAwait(false);

                    await endpointDependencyContainer
                        .InvokeWithinTransaction(false, modelProvider, CheckEmptyRows, token)
                        .ConfigureAwait(false);
                };
            }

            static async Task CheckRows(
                IAdvancedDatabaseTransaction transaction,
                IModelProvider modelProvider,
                CancellationToken token)
            {
                Assert.True(await transaction.All<InboxMessage>().CachedExpression("75B52E36-C22C-4FAE-BA89-E67C232ED2BE").AnyAsync(token).ConfigureAwait(false));
                Assert.True(await transaction.All<OutboxMessage>().CachedExpression("D50AA461-3C90-42BA-AF90-FD0E059562BA").AnyAsync(token).ConfigureAwait(false));
                Assert.True(await transaction.All<IntegrationMessage>().CachedExpression("C2F0D883-68FB-4887-B11E-54E2F835E552").AnyAsync(token).ConfigureAwait(false));
                Assert.True(await transaction.All<IntegrationMessageHeader>().CachedExpression("CCAEB0A3-B95E-45D1-88FA-609B91F4737B").AnyAsync(token).ConfigureAwait(false));
                Assert.True(await transaction.AllMtm<IntegrationMessage, IntegrationMessageHeader, Guid, Guid>(modelProvider, message => message.Headers).Cast<IUniqueIdentified>().CachedExpression("04494178-C124-4BF1-8841-DEA3427A3E99").AnyAsync(token).ConfigureAwait(false));

                var rowsCount = await transaction.All<IntegrationMessage>().CachedExpression("37A34847-5A14-4D4E-928A-75683BBB1514").CountAsync(token).ConfigureAwait(false);

                Assert.Equal(3, rowsCount);
            }

            static async Task Delete(
                IAdvancedDatabaseTransaction transaction,
                CancellationToken token)
            {
                var affectedRowsCount = await transaction
                    .Delete<IntegrationMessage>()
                    .Where(_ => true)
                    .CachedExpression("4C8F330F-9142-486C-90BE-6F76B262487A")
                    .Invoke(token)
                    .ConfigureAwait(false);

                Assert.Equal(3, affectedRowsCount);
            }

            static async Task CheckEmptyRows(
                IAdvancedDatabaseTransaction transaction,
                IModelProvider modelProvider,
                CancellationToken token)
            {
                Assert.False(await transaction.All<InboxMessage>().CachedExpression("75B52E36-C22C-4FAE-BA89-E67C232ED2BE").AnyAsync(token).ConfigureAwait(false));
                Assert.False(await transaction.All<OutboxMessage>().CachedExpression("D50AA461-3C90-42BA-AF90-FD0E059562BA").AnyAsync(token).ConfigureAwait(false));
                Assert.False(await transaction.All<IntegrationMessage>().CachedExpression("C2F0D883-68FB-4887-B11E-54E2F835E552").AnyAsync(token).ConfigureAwait(false));
                Assert.False(await transaction.All<IntegrationMessageHeader>().CachedExpression("CCAEB0A3-B95E-45D1-88FA-609B91F4737B").AnyAsync(token).ConfigureAwait(false));
                Assert.False(await transaction.AllMtm<IntegrationMessage, IntegrationMessageHeader, Guid, Guid>(modelProvider, message => message.Headers).Cast<IUniqueIdentified>().CachedExpression("04494178-C124-4BF1-8841-DEA3427A3E99").AnyAsync(token).ConfigureAwait(false));
            }
        }
    }
}