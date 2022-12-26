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
    using CrossCuttingConcerns.Extensions;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Api.Exceptions;
    using DataAccess.Api.Persisting;
    using DataAccess.Api.Transaction;
    using DataAccess.Orm.PostgreSql.Extensions;
    using DataAccess.Orm.Sql.Settings;
    using DatabaseEntities;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Authorization;
    using GenericEndpoint.Authorization.Host;
    using GenericEndpoint.DataAccess.Host;
    using GenericEndpoint.DataAccess.Settings;
    using GenericEndpoint.EventSourcing;
    using GenericEndpoint.EventSourcing.Host;
    using GenericEndpoint.Host;
    using GenericEndpoint.Host.Builder;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;
    using GenericEndpoint.Pipeline;
    using GenericEndpoint.Settings;
    using IntegrationTransport.Host;
    using IntegrationTransport.RabbitMQ.Settings;
    using IntegrationTransport.RpcRequest;
    using JwtAuthentication;
    using MessageHandlers;
    using Messages;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Migrations;
    using Mocks;
    using Registrations;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// DataAccessTest
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
    public class DataAccessTest : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public DataAccessTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// Test cases for DataAccessTest
        /// </summary>
        /// <returns>Test cases</returns>
        public static IEnumerable<object[]> DataAccessTestData()
        {
            var timeout = TimeSpan.FromSeconds(60);

            Func<string, DirectoryInfo> settingsDirectoryProducer =
                testDirectory => SolutionExtensions
                   .ProjectFile()
                   .Directory
                   .EnsureNotNull("Project directory wasn't found")
                   .StepInto("Settings")
                   .StepInto(testDirectory);

            var useInMemoryIntegrationTransport = new Func<DirectoryInfo, IsolationLevel, IHostBuilder, Func<IEndpointBuilder, IEndpointBuilder>, IHostBuilder>(
                static (settingsDirectory, isolationLevel, hostBuilder, options) => hostBuilder
                   .UseIntegrationTransport(builder => options(builder
                       .WithInMemoryIntegrationTransport(hostBuilder)
                       .ModifyContainerOptions(options => options
                           .WithManualRegistrations(new MessagesCollectorManualRegistration())
                           .WithManualRegistrations(new VirtualHostManualRegistration(settingsDirectory.Name + isolationLevel))))
                       .BuildOptions()));

            var useRabbitMqIntegrationTransport = new Func<DirectoryInfo, IsolationLevel, IHostBuilder, Func<IEndpointBuilder, IEndpointBuilder>, IHostBuilder>(
                static (settingsDirectory, isolationLevel, hostBuilder, options) => hostBuilder
                   .UseIntegrationTransport(builder => options(builder
                       .WithRabbitMqIntegrationTransport(hostBuilder)
                       .ModifyContainerOptions(options => options
                           .WithManualRegistrations(new PurgeRabbitMqQueuesManualRegistration())
                           .WithManualRegistrations(new MessagesCollectorManualRegistration())
                           .WithManualRegistrations(new VirtualHostManualRegistration(settingsDirectory.Name + isolationLevel))))
                       .BuildOptions()));

            var integrationTransportProviders = new[]
            {
                useInMemoryIntegrationTransport,
                useRabbitMqIntegrationTransport
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
               .SelectMany(useTransport => dataAccessProviders
                   .SelectMany(withDataAccess => eventSourcingProviders
                       .SelectMany(withEventSourcing => isolationLevels
                           .Select(isolationLevel => new object[]
                           {
                               settingsDirectoryProducer,
                               useTransport,
                               withDataAccess,
                               withEventSourcing,
                               isolationLevel,
                               timeout
                           }))));
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(DataAccessTestData))]
        internal async Task BackgroundOutboxDeliveryTest(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            Func<DirectoryInfo, IsolationLevel, IHostBuilder, Func<IEndpointBuilder, IEndpointBuilder>, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            IsolationLevel isolationLevel,
            TimeSpan timeout)
        {
            var settingsDirectory = settingsDirectoryProducer(nameof(BackgroundOutboxDeliveryTest));

            var messageTypes = new[]
            {
                typeof(Request),
                typeof(Reply)
            };

            var messageHandlerTypes = new[]
            {
                typeof(AlwaysReplyMessageHandler)
            };

            var manualMigrations = new[]
            {
                typeof(RecreatePostgreSqlDatabaseMigration)
            };

            var additionalOurTypes = messageTypes
               .Concat(messageHandlerTypes)
               .Concat(manualMigrations)
               .ToArray();

            var host = useTransport(settingsDirectory, isolationLevel, Fixture.CreateHostBuilder(Output), static builder => builder)
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithManualRegistrations(new IsolationLevelManualRegistration(isolationLevel))
                           .WithManualRegistrations(new BackgroundOutboxDeliveryManualRegistration()))
                       .BuildOptions())
               .BuildHost(settingsDirectory);

            await RunHostTest.RunTestHost(Output,
                    host,
                    BackgroundOutboxDeliveryTestInternal(settingsDirectory, settingsDirectory.Name + isolationLevel, isolationLevel),
                    timeout)
               .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> BackgroundOutboxDeliveryTestInternal(
                DirectoryInfo settingsDirectory,
                string virtualHost,
                IsolationLevel isolationLevel)
            {
                return async (_, host, token) =>
                {
                    var transportDependencyContainer = host.GetTransportDependencyContainer();
                    var endpointDependencyContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                    var sqlDatabaseSettings = await endpointDependencyContainer
                       .Resolve<ISettingsProvider<SqlDatabaseSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(settingsDirectory.Name, sqlDatabaseSettings.Database);
                    Assert.Equal(isolationLevel, sqlDatabaseSettings.IsolationLevel);
                    Assert.Equal(1u, sqlDatabaseSettings.ConnectionPoolSize);

                    var rabbitMqSettings = await transportDependencyContainer
                       .Resolve<ISettingsProvider<RabbitMqSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(virtualHost, rabbitMqSettings.VirtualHost);

                    Reply reply;

                    var outboxSettings = await endpointDependencyContainer
                       .Resolve<ISettingsProvider<OutboxSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(TimeSpan.FromSeconds(1), outboxSettings.OutboxDeliveryInterval);

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                        reply = await integrationContext
                           .RpcRequest<Request, Reply>(new Request(42), token)
                           .ConfigureAwait(false);
                    }

                    Assert.Equal(42, reply.Id);
                };
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(DataAccessTestData))]
        internal async Task OptimisticConcurrencyTest(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            Func<DirectoryInfo, IsolationLevel, IHostBuilder, Func<IEndpointBuilder, IEndpointBuilder>, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            IsolationLevel isolationLevel,
            TimeSpan timeout)
        {
            var settingsDirectory = settingsDirectoryProducer(nameof(OptimisticConcurrencyTest));

            var databaseEntities = new[]
            {
                typeof(DatabaseEntity)
            };

            var manualMigrations = new[]
            {
                typeof(RecreatePostgreSqlDatabaseMigration)
            };

            var additionalOurTypes = databaseEntities
               .Concat(manualMigrations)
               .ToArray();

            var host = useTransport(settingsDirectory, isolationLevel, Fixture.CreateHostBuilder(Output), static builder => builder)
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithManualRegistrations(new IsolationLevelManualRegistration(isolationLevel)))
                       .BuildOptions())
               .BuildHost(settingsDirectory);

            await RunHostTest.RunTestHost(Output,
                    host,
                    OptimisticConcurrencyTestInternal(settingsDirectory, settingsDirectory.Name + isolationLevel, isolationLevel),
                    timeout)
               .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> OptimisticConcurrencyTestInternal(
                DirectoryInfo settingsDirectory,
                string virtualHost,
                IsolationLevel isolationLevel)
            {
                return async (_, host, token) =>
                {
                    var transportDependencyContainer = host.GetTransportDependencyContainer();
                    var endpointDependencyContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                    var sqlDatabaseSettings = await endpointDependencyContainer
                       .Resolve<ISettingsProvider<SqlDatabaseSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(settingsDirectory.Name, sqlDatabaseSettings.Database);
                    Assert.Equal(isolationLevel, sqlDatabaseSettings.IsolationLevel);
                    Assert.Equal(3u, sqlDatabaseSettings.ConnectionPoolSize);

                    var rabbitMqSettings = await transportDependencyContainer
                       .Resolve<ISettingsProvider<RabbitMqSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(virtualHost, rabbitMqSettings.VirtualHost);

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
                           .Write<DatabaseEntity>()
                           .Insert(new[] { entity }, EnInsertBehavior.Default, token)
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
                           .Read<DatabaseEntity>()
                           .SingleOrDefaultAsync(primaryKey, token)
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
                           .Write<DatabaseEntity>()
                           .Update(entity => entity.IntField,
                                entity => entity.IntField + 1,
                                entity => entity.PrimaryKey == primaryKey,
                                token)
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
                           .Write<DatabaseEntity>()
                           .Delete(entity => entity.PrimaryKey == primaryKey, token)
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
        [MemberData(nameof(DataAccessTestData))]
        internal async Task ReactiveTransactionalStoreTest(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            Func<DirectoryInfo, IsolationLevel, IHostBuilder, Func<IEndpointBuilder, IEndpointBuilder>, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            IsolationLevel isolationLevel,
            TimeSpan timeout)
        {
            var settingsDirectory = settingsDirectoryProducer(nameof(ReactiveTransactionalStoreTest));

            var databaseEntities = new[]
            {
                typeof(DatabaseEntity)
            };

            var manualMigrations = new[]
            {
                typeof(RecreatePostgreSqlDatabaseMigration)
            };

            var additionalOurTypes = databaseEntities
               .Concat(manualMigrations)
               .ToArray();

            var host = useTransport(settingsDirectory, isolationLevel, Fixture.CreateHostBuilder(Output), static builder => builder)
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithManualRegistrations(new IsolationLevelManualRegistration(isolationLevel)))
                       .BuildOptions())
               .BuildHost(settingsDirectory);

            await RunHostTest.RunTestHost(
                    Output,
                    host,
                    ReactiveTransactionalStoreTestInternal(settingsDirectory, settingsDirectory.Name + isolationLevel, isolationLevel),
                    timeout)
               .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> ReactiveTransactionalStoreTestInternal(
                DirectoryInfo settingsDirectory,
                string virtualHost,
                IsolationLevel isolationLevel)
            {
                return async (_, host, token) =>
                {
                    var transportDependencyContainer = host.GetTransportDependencyContainer();
                    var endpointDependencyContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);

                    var sqlDatabaseSettings = await endpointDependencyContainer
                       .Resolve<ISettingsProvider<SqlDatabaseSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(settingsDirectory.Name, sqlDatabaseSettings.Database);
                    Assert.Equal(isolationLevel, sqlDatabaseSettings.IsolationLevel);
                    Assert.Equal(1u, sqlDatabaseSettings.ConnectionPoolSize);

                    var rabbitMqSettings = await transportDependencyContainer
                       .Resolve<ISettingsProvider<RabbitMqSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(virtualHost, rabbitMqSettings.VirtualHost);

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
                   .Write<DatabaseEntity>()
                   .Insert(new[] { entity }, EnInsertBehavior.Default, token)
                   .ConfigureAwait(false);
            }

            static async Task<DatabaseEntity?> ReadEntity(
                IDatabaseTransaction transaction,
                Guid primaryKey,
                CancellationToken token)
            {
                return await transaction
                   .Read<DatabaseEntity>()
                   .SingleOrDefaultAsync(primaryKey, token)
                   .ConfigureAwait(false);
            }

            static async Task UpdateEntity(
                IDatabaseTransaction transaction,
                Guid primaryKey,
                CancellationToken token)
            {
                _ = await transaction
                   .Write<DatabaseEntity>()
                   .Update(entity => entity.IntField,
                        entity => entity.IntField + 1,
                        entity => entity.PrimaryKey == primaryKey,
                        token)
                   .ConfigureAwait(false);
            }

            static async Task DeleteEntity(
                IDatabaseTransaction transaction,
                Guid primaryKey,
                CancellationToken token)
            {
                _ = await transaction
                   .Write<DatabaseEntity>()
                   .Delete(entity => entity.PrimaryKey == primaryKey, token)
                   .ConfigureAwait(false);
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(DataAccessTestData))]
        internal async Task OnlyCommandsCanIntroduceChanges(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            Func<DirectoryInfo, IsolationLevel, IHostBuilder, Func<IEndpointBuilder, IEndpointBuilder>, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            IsolationLevel isolationLevel,
            TimeSpan timeout)
        {
            var settingsDirectory = settingsDirectoryProducer(nameof(OnlyCommandsCanIntroduceChanges));

            var messageTypes = new[]
            {
                typeof(Command),
                typeof(Request),
                typeof(Reply),
                typeof(TransportEvent)
            };

            var messageHandlerTypes = new Type[]
            {
                typeof(CommandIntroduceDatabaseChanges),
                typeof(RequestIntroduceDatabaseChanges),
                typeof(ReplyIntroduceDatabaseChanges),
                typeof(TransportEventIntroduceDatabaseChanges)
            };

            var databaseEntities = new[]
            {
                typeof(DatabaseEntity)
            };

            var manualMigrations = new[]
            {
                typeof(RecreatePostgreSqlDatabaseMigration)
            };

            var additionalOurTypes = messageTypes
               .Concat(messageHandlerTypes)
               .Concat(databaseEntities)
               .Concat(manualMigrations)
               .ToArray();

            var host = useTransport(settingsDirectory, isolationLevel, Fixture.CreateHostBuilder(Output), static builder => builder)
               .UseEndpoint(TestIdentity.Endpoint10,
                    (_, builder) => withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
                       .ModifyContainerOptions(options => options
                           .WithAdditionalOurTypes(additionalOurTypes)
                           .WithManualRegistrations(new IsolationLevelManualRegistration(isolationLevel)))
                       .BuildOptions())
               .BuildHost(settingsDirectory);

            await RunHostTest.RunTestHost(Output,
                    host,
                    OnlyCommandsCanIntroduceChangesInternal(settingsDirectory, settingsDirectory.Name + isolationLevel, isolationLevel),
                    timeout)
               .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> OnlyCommandsCanIntroduceChangesInternal(
                DirectoryInfo settingsDirectory,
                string virtualHost,
                IsolationLevel isolationLevel)
            {
                return async (output, host, token) =>
                {
                    var transportDependencyContainer = host.GetTransportDependencyContainer();
                    var endpointDependencyContainer = host.GetEndpointDependencyContainer(TestIdentity.Endpoint10);
                    var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                    var sqlDatabaseSettings = await endpointDependencyContainer
                       .Resolve<ISettingsProvider<SqlDatabaseSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(settingsDirectory.Name, sqlDatabaseSettings.Database);
                    Assert.Equal(isolationLevel, sqlDatabaseSettings.IsolationLevel);
                    Assert.Equal(1u, sqlDatabaseSettings.ConnectionPoolSize);

                    var genericEndpointSettings = await endpointDependencyContainer
                       .Resolve<ISettingsProvider<GenericEndpointSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(1u, genericEndpointSettings.RpcRequestSecondsTimeout);

                    var rabbitMqSettings = await transportDependencyContainer
                       .Resolve<ISettingsProvider<RabbitMqSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(virtualHost, rabbitMqSettings.VirtualHost);

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                        var awaiter = collector.WaitUntilMessageIsNotReceived(message => message.Payload is Command);

                        await integrationContext
                           .Send(new Command(42), token)
                           .ConfigureAwait(false);

                        await awaiter.ConfigureAwait(false);

                        Assert.Empty(collector.ErrorMessages);
                        collector.ErrorMessages.Clear();
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                        var awaiter = collector.WaitUntilErrorMessageIsNotReceived(message => message.Payload is Request);

                        await integrationContext
                            .Request<Request, Reply>(new Request(42), token)
                            .ConfigureAwait(false);

                        await awaiter.ConfigureAwait(false);

                        Assert.Single(collector.ErrorMessages);
                        Assert.True(collector.ErrorMessages.Single().exception is InvalidOperationException exception && exception.Message.Contains("only commands can introduce changes", StringComparison.OrdinalIgnoreCase));
                        collector.ErrorMessages.Clear();
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                        var awaiter = collector.WaitUntilErrorMessageIsNotReceived(message => message.Payload is Request);

                        var rpcException = default(Exception?);

                        try
                        {
                            _ = await integrationContext
                               .RpcRequest<Request, Reply>(new Request(42), token)
                               .ConfigureAwait(false);
                        }
                        catch (InvalidOperationException invalidOperationException)
                        {
                            rpcException = invalidOperationException;
                        }

                        await awaiter.ConfigureAwait(false);

                        Assert.Single(collector.ErrorMessages);
                        Assert.True(collector.ErrorMessages.Single().exception is InvalidOperationException exception && exception.Message.Contains("only commands can introduce changes", StringComparison.OrdinalIgnoreCase));
                        Assert.Equal(collector.ErrorMessages.Single().exception.GetType(), rpcException.GetType());
                        Assert.Equal(collector.ErrorMessages.Single().exception.Message, rpcException.Message);
                        collector.ErrorMessages.Clear();
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var integrationMessageFactory = transportDependencyContainer.Resolve<IIntegrationMessageFactory>();
                        var request = new Request(42);
                        var initiatorMessage = integrationMessageFactory.CreateGeneralMessage(request, typeof(Request), new[] { new SentFrom(TestIdentity.Endpoint10) }, null);
                        var integrationContext = transportDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(initiatorMessage);

                        var awaiter = collector.WaitUntilErrorMessageIsNotReceived(message => message.Payload is Reply);

                        await integrationContext
                           .Reply(request, new Reply(request.Id), token)
                           .ConfigureAwait(false);

                        await awaiter.ConfigureAwait(false);

                        Assert.Single(collector.ErrorMessages);
                        Assert.True(collector.ErrorMessages.Single().exception is InvalidOperationException exception && exception.Message.Contains("only commands can introduce changes", StringComparison.OrdinalIgnoreCase));
                        collector.ErrorMessages.Clear();
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        var integrationContext = transportDependencyContainer.Resolve<IIntegrationContext>();

                        var awaiter = collector.WaitUntilErrorMessageIsNotReceived(message => message.Payload is TransportEvent);

                        await integrationContext
                           .Publish(new TransportEvent(42), token)
                           .ConfigureAwait(false);

                        await awaiter.ConfigureAwait(false);

                        Assert.Single(collector.ErrorMessages);
                        Assert.True(collector.ErrorMessages.Single().exception is InvalidOperationException exception && exception.Message.Contains("only commands can introduce changes", StringComparison.OrdinalIgnoreCase));
                        collector.ErrorMessages.Clear();
                    }
                };
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(DataAccessTestData))]
        internal async Task AuthenticateUserTest(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            Func<DirectoryInfo, IsolationLevel, IHostBuilder, Func<IEndpointBuilder, IEndpointBuilder>, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            IsolationLevel isolationLevel,
            TimeSpan timeout)
        {
            var settingsDirectory = settingsDirectoryProducer(nameof(AuthenticateUserTest));

            var additionalOurTypes = new[]
            {
                typeof(RecreatePostgreSqlDatabaseMigration)
            };

            var host = useTransport(settingsDirectory, isolationLevel, Fixture.CreateHostBuilder(Output), builder => builder.WithAuthorization())
               .UseAuthEndpoint(builder => withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
                   .ModifyContainerOptions(options => options
                       .WithAdditionalOurTypes(additionalOurTypes)
                       .WithManualRegistrations(new IsolationLevelManualRegistration(isolationLevel)))
                   .BuildOptions())
               .BuildHost(settingsDirectory);

            await RunHostTest.RunTestHost(
                    Output,
                    host,
                    AuthorizeUserTestInternal(settingsDirectory, settingsDirectory.Name + isolationLevel, isolationLevel),
                    timeout)
               .ConfigureAwait(false);

            static Func<ITestOutputHelper, IHost, CancellationToken, Task> AuthorizeUserTestInternal(
                DirectoryInfo settingsDirectory,
                string virtualHost,
                IsolationLevel isolationLevel)
            {
                return async (output, host, token) =>
                {
                    var transportDependencyContainer = host.GetTransportDependencyContainer();
                    var endpointDependencyContainer = host.GetEndpointDependencyContainer(AuthEndpoint.Contract.Identity.LogicalName);
                    var collector = transportDependencyContainer.Resolve<TestMessagesCollector>();

                    var sqlDatabaseSettings = await endpointDependencyContainer
                       .Resolve<ISettingsProvider<SqlDatabaseSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(settingsDirectory.Name, sqlDatabaseSettings.Database);
                    Assert.Equal(isolationLevel, sqlDatabaseSettings.IsolationLevel);
                    Assert.Equal(1u, sqlDatabaseSettings.ConnectionPoolSize);

                    var rabbitMqSettings = await transportDependencyContainer
                       .Resolve<ISettingsProvider<RabbitMqSettings>>()
                       .Get(token)
                       .ConfigureAwait(false);

                    Assert.Equal(virtualHost, rabbitMqSettings.VirtualHost);

                    var username = "qwerty";
                    var password = "12345678";

                    var authorizationToken = transportDependencyContainer
                        .Resolve<ITokenProvider>()
                        .GenerateToken(username, new[] { AuthEndpoint.Contract.Features.Authentication, GenericEndpoint.EventSourcing.Features.EventSourcing }, TimeSpan.FromSeconds(60));

                    var initiatorMessage = transportDependencyContainer
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
                        IIntegrationContext integrationContext = transportDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(initiatorMessage);

                        userAuthenticationResult = await integrationContext
                           .RpcRequest<AuthenticateUser, UserAuthenticationResult>(request, CancellationToken.None)
                           .ConfigureAwait(false);
                    }

                    output.WriteLine(userAuthenticationResult.ShowProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty));

                    Assert.Equal(username, userAuthenticationResult.Username);
                    Assert.Empty(userAuthenticationResult.Token);

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        IIntegrationContext integrationContext = transportDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(initiatorMessage);

                        var awaiter = Task.WhenAll(
                            collector.WaitUntilMessageIsNotReceived<CreateUser>(),
                            collector.WaitUntilMessageIsNotReceived<CaptureDomainEvent<AuthEndpoint.Domain.Model.User, AuthEndpoint.Domain.Model.UserWasCreated>>(),
                            collector.WaitUntilMessageIsNotReceived<UserWasCreated>());

                        await integrationContext
                           .Send(new CreateUser(username, password), token)
                           .ConfigureAwait(false);

                        await awaiter.ConfigureAwait(false);
                    }

                    await using (transportDependencyContainer.OpenScopeAsync().ConfigureAwait(false))
                    {
                        IIntegrationContext integrationContext = transportDependencyContainer.Resolve<IAdvancedIntegrationContext, IntegrationMessage>(initiatorMessage);

                        userAuthenticationResult = await integrationContext
                           .RpcRequest<AuthenticateUser, UserAuthenticationResult>(request, CancellationToken.None)
                           .ConfigureAwait(false);
                    }

                    output.WriteLine(userAuthenticationResult.ShowProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty));

                    Assert.Equal(username, userAuthenticationResult.Username);
                    Assert.NotEmpty(userAuthenticationResult.Token);
                };
            }
        }
    }
}