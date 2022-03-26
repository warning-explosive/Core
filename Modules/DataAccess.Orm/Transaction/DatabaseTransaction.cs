namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System;
    using System.Collections.Concurrent;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Persisting;
    using Api.Reading;
    using Api.Transaction;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Primitives;
    using CompositionRoot.Api.Abstractions.Container;
    using Connection;
    using GenericDomain.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class DatabaseTransaction : IAdvancedDatabaseTransaction, IDisposable
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseConnectionProvider _connectionProvider;
        private readonly IChangesTracker _changesTracker;
        private readonly ITransactionalStore _transactionalStore;

        private static readonly ConcurrentDictionary<string, IDbConnection?> Connections
            = new ConcurrentDictionary<string, IDbConnection?>();

        [SuppressMessage("Analysis", "CA2213", Justification = "disposed with Interlocked.Exchange")]
        private IDbTransaction? _transaction;

        public DatabaseTransaction(
            IDependencyContainer dependencyContainer,
            IDatabaseConnectionProvider connectionProvider,
            IChangesTracker changesTracker,
            ITransactionalStore transactionalStore)
        {
            _dependencyContainer = dependencyContainer;
            _connectionProvider = connectionProvider;
            _changesTracker = changesTracker;
            _transactionalStore = transactionalStore;

            _transaction = null;
        }

        public bool HasChanges => _changesTracker.HasChanges;

        public IDbTransaction UnderlyingDbTransaction
        {
            get
            {
                _transaction ??= Open();
                return _transaction;
            }
        }

        private IDbConnection UnderlyingDbConnection
        {
            get
            {
                var connection = Connections.GetOrAdd(_connectionProvider.Database, _ => default);

                for (var i = 0; i < 3; i++)
                {
                    switch (connection?.State)
                    {
                        case ConnectionState.Connecting:
                            Task.Delay(TimeSpan.FromMilliseconds(50));
                            continue;
                        case ConnectionState.Open:
                        case ConnectionState.Executing:
                        case ConnectionState.Fetching:
                            return connection;
                        case ConnectionState.Broken:
                        case ConnectionState.Closed:
                        case null:
                        default:
                            break;
                    }
                }

                connection?.Dispose();

                connection = _connectionProvider
                    .OpenConnection(CancellationToken.None)
                    .Result;

                Connections[_connectionProvider.Database] = connection;

                return connection;
            }
        }

        public IReadRepository<TEntity, TKey> Read<TEntity, TKey>()
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            return _dependencyContainer.Resolve<IReadRepository<TEntity, TKey>>();
        }

        public IRepository<TEntity, TKey> Write<TEntity, TKey>()
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            return _dependencyContainer.Resolve<IRepository<TEntity, TKey>>();
        }

        public IBulkRepository<TEntity, TKey> BulkWrite<TEntity, TKey>()
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            return _dependencyContainer.Resolve<IBulkRepository<TEntity, TKey>>();
        }

        public async Task<IAsyncDisposable> OpenScope(bool commit, CancellationToken token)
        {
            await Open(token).ConfigureAwait(false);

            return AsyncDisposable.Create((commit, token), state => Close(state.commit, state.token));
        }

        public Task Open(CancellationToken token)
        {
            _transaction = Open();

            return Task.CompletedTask;
        }

        public async Task Close(bool commit, CancellationToken token)
        {
            try
            {
                if (commit)
                {
                    await _changesTracker
                        .SaveChanges(token)
                        .ConfigureAwait(false);

                    _transaction?.Commit();
                }
                else
                {
                    _transaction?.Rollback();
                }
            }
            finally
            {
                _transaction?.Dispose();
                Interlocked.Exchange(ref _transaction, default)?.Dispose();
                _changesTracker.Dispose();
                _transactionalStore.Dispose();
            }
        }

        public void Dispose()
        {
            Close(false, CancellationToken.None).Wait();
        }

        public Task Track(IAggregate aggregate, CancellationToken token)
        {
            return _changesTracker.Track(aggregate, token);
        }

        public void Store<TEntity, TKey>(TEntity entity)
            where TEntity : IDatabaseEntity<TKey>
            where TKey : notnull
        {
            _transactionalStore.Store(entity, static e => e.PrimaryKey);
        }

        public bool TryGetValue<TEntity, TKey>(TKey key, [NotNullWhen(true)] out TEntity? entity)
            where TEntity : IDatabaseEntity<TKey>
            where TKey : notnull
        {
            return _transactionalStore.TryGetValue(key, out entity);
        }

        private IDbTransaction Open()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Database transaction have already been opened");
            }

            return UnderlyingDbConnection.BeginTransaction(_connectionProvider.IsolationLevel);
        }
    }
}