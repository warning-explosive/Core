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
    using DataAccess.Orm.Sql.Exceptions;
    using DataAccess.Orm.Sql.Linq;
    using DataAccess.Orm.Sql.Model;
    using DataAccess.Orm.Sql.Model.Attributes;
    using DataAccess.Orm.Sql.Postgres.Connection;
    using DataAccess.Orm.Sql.Settings;
    using DataAccess.Orm.Sql.Transaction;
    using DatabaseEntities;
    using Extensions;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Authorization;
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
        [MemberData(nameof(DataAccessTestData))]
        internal async Task BackgroundOutboxDeliveryTest(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, DirectoryInfo, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            IsolationLevel isolationLevel)
        {
            var settingsDirectory = settingsDirectoryProducer(TestCase.Method.Name);

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
                    (_, builder) => withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
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
        [MemberData(nameof(DataAccessTestData))]
        internal async Task OptimisticConcurrencyTest(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, DirectoryInfo, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            IsolationLevel isolationLevel)
        {
            var settingsDirectory = settingsDirectoryProducer(TestCase.Method.Name);

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
                    (_, builder) => withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
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
        [MemberData(nameof(DataAccessTestData))]
        internal async Task ReactiveTransactionalStoreTest(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, DirectoryInfo, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            IsolationLevel isolationLevel)
        {
            var settingsDirectory = settingsDirectoryProducer(TestCase.Method.Name);

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
                    (_, builder) => withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
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
        [MemberData(nameof(DataAccessTestData))]
        internal async Task OnlyCommandsCanIntroduceChanges(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, DirectoryInfo, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            IsolationLevel isolationLevel)
        {
            var settingsDirectory = settingsDirectoryProducer(TestCase.Method.Name);

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
                    (_, builder) => withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
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

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(DataAccessTestData))]
        internal async Task AuthenticateUserTest(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, DirectoryInfo, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            IsolationLevel isolationLevel)
        {
            var settingsDirectory = settingsDirectoryProducer(TestCase.Method.Name);

            var startupActions = new[]
            {
                typeof(RecreatePostgreSqlDatabaseHostedServiceStartupAction)
            };

            var host = Fixture
                .CreateHostBuilder()
                .UseIntegrationTransport(transportIdentity, useTransport, settingsDirectory)
                .UseAuthEndpoint((_, builder) =>
                    withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
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
        [MemberData(nameof(DataAccessTestData))]
        internal async Task CascadeDeleteTest(
            Func<string, DirectoryInfo> settingsDirectoryProducer,
            TransportIdentity transportIdentity,
            Func<IHostBuilder, TransportIdentity, DirectoryInfo, IHostBuilder> useTransport,
            Func<IEndpointBuilder, Action<DataAccessOptions>?, IEndpointBuilder> withDataAccess,
            Func<IEndpointBuilder, IEndpointBuilder> withEventSourcing,
            IsolationLevel isolationLevel)
        {
            var settingsDirectory = settingsDirectoryProducer(TestCase.Method.Name);

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
                    (_, builder) => withEventSourcing(withDataAccess(builder, options => options.ExecuteMigrations()))
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

                        // TODO: #225 - simplify sending
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