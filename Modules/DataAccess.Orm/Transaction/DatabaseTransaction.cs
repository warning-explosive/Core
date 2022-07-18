namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Persisting;
    using Api.Reading;
    using Api.Transaction;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Primitives;
    using CompositionRoot.Api.Abstractions;
    using Connection;

    [Component(EnLifestyle.Scoped)]
    internal class DatabaseTransaction : IAdvancedDatabaseTransaction,
                                         IDisposable,
                                         IResolvable<IAdvancedDatabaseTransaction>,
                                         IResolvable<IDatabaseTransactionStore>,
                                         IResolvable<IDatabaseTransaction>,
                                         IResolvable<IDatabaseContext>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseConnectionProvider _connectionProvider;

        private readonly ITransactionalStore _transactionalStore;
        private readonly List<ITransactionalChange> _changes;

        [SuppressMessage("Analysis", "CA2213", Justification = "disposed with Interlocked.Exchange")]
        private IDatabaseConnection? _connection;

        [SuppressMessage("Analysis", "CA2213", Justification = "disposed with Interlocked.Exchange")]
        private IDbTransaction? _transaction;

        public DatabaseTransaction(
            IDependencyContainer dependencyContainer,
            IDatabaseConnectionProvider connectionProvider,
            ITransactionalStore transactionalStore)
        {
            _dependencyContainer = dependencyContainer;
            _connectionProvider = connectionProvider;

            _transactionalStore = transactionalStore;
            _changes = new List<ITransactionalChange>();
        }

        public bool HasChanges => _changes.Any();

        public IDbTransaction DbTransaction
        {
            get
            {
                _transaction ??= Open();
                return _transaction;
            }
        }

        public IDatabaseConnection DbConnection
        {
            get
            {
                for (var i = 0; i < 3; i++)
                {
                    switch (_connection?.UnderlyingDbConnection.State)
                    {
                        case ConnectionState.Connecting:
                            Task.Delay(TimeSpan.FromMilliseconds(50));
                            continue;
                        case ConnectionState.Open:
                        case ConnectionState.Executing:
                        case ConnectionState.Fetching:
                            return _connection;
                        case ConnectionState.Broken:
                        case ConnectionState.Closed:
                        case null:
                        default:
                            break;
                    }
                }

                _connection?.Dispose();

                _connection = _connectionProvider
                    .OpenConnection(CancellationToken.None)
                    .Result;

                return _connection;
            }
        }

        public bool Connected => _connection?.UnderlyingDbConnection != null;

        public IReadRepository<TEntity, TKey> Read<TEntity, TKey>()
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            return _dependencyContainer.Resolve<IReadRepository<TEntity, TKey>>();
        }

        public IRepository<TEntity, TKey> Write<TEntity, TKey>()
            where TEntity : IDatabaseEntity<TKey>
            where TKey : notnull
        {
            return _dependencyContainer.Resolve<IRepository<TEntity, TKey>>();
        }

        public IRepository Write()
        {
            return _dependencyContainer.Resolve<IRepository>();
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
            ICollection<ITransactionalChange> changes;

            try
            {
                changes = commit
                    ? _changes.ToList()
                    : Array.Empty<ITransactionalChange>();

                _transaction?.Rollback();
            }
            finally
            {
                _changes.Clear();
                _transactionalStore.Clear();

                Interlocked.Exchange(ref _transaction, default)?.Dispose();
                Interlocked.Exchange(ref _connection, default)?.Dispose();
            }

            if (!changes.Any())
            {
                return;
            }

            try
            {
                foreach (var change in changes)
                {
                    await change
                       .Apply(this, token)
                       .ConfigureAwait(false);
                }

                _transaction?.Commit();
            }
            finally
            {
                _changes.Clear();
                _transactionalStore.Clear();

                Interlocked.Exchange(ref _transaction, default)?.Dispose();
                Interlocked.Exchange(ref _connection, default)?.Dispose();
            }
        }

        public void Dispose()
        {
            Close(false, CancellationToken.None).Wait();
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

        public void CollectChange(ITransactionalChange change)
        {
            // TODO: #133 - update transactional store
            // TODO: #133 - transactional store update test
            // TODO: #133 - optimistic concurrency test
            _changes.Add(change);
        }

        private IDbTransaction Open()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Database transaction have already been opened");
            }

            return DbConnection
               .UnderlyingDbConnection
               .BeginTransaction(_connectionProvider.IsolationLevel);
        }
    }
}