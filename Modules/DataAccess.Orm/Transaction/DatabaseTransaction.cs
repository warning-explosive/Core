namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System;
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
    using ChangesTracking;
    using CompositionRoot.Api.Abstractions.Container;
    using Connection;
    using GenericDomain.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class DatabaseTransaction : IAdvancedDatabaseTransaction, IDisposable
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseConnectionProvider _connectionProvider;
        private readonly IChangesTracker _changesTracker;

        private static IDbConnection? _connection;

        [SuppressMessage("Analysis", "CA2213", Justification = "disposed with Interlocked.Exchange")]
        private IDbTransaction? _transaction;

        public DatabaseTransaction(
            IDependencyContainer dependencyContainer,
            IDatabaseConnectionProvider connectionProvider,
            IChangesTracker changesTracker)
        {
            _dependencyContainer = dependencyContainer;
            _connectionProvider = connectionProvider;
            _changesTracker = changesTracker;
        }

        public bool HasChanges => _changesTracker.HasChanges;

        public IDbTransaction UnderlyingDbTransaction
        {
            get
            {
                _transaction ??= Open(CancellationToken.None).Result;
                return _transaction;
            }
        }

        private IDbConnection UnderlyingDbConnection
        {
            get
            {
                for (var i = 0; i < 3; i++)
                {
                    switch (_connection?.State)
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

                Interlocked.Exchange(ref _connection, default)?.Dispose();

                _connection = _connectionProvider
                    .OpenConnection(CancellationToken.None)
                    .Result;

                return _connection;
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

        public Task<IDbTransaction> Open(CancellationToken token)
        {
            return Open(_connectionProvider.IsolationLevel, token);
        }

        public Task<IDbTransaction> Open(IsolationLevel isolationLevel, CancellationToken token)
        {
            // TODO: #150 - support manually opened nested transactions
            // TODO: #151 - work with single, app wide connection
            if (_transaction != null)
            {
                throw new InvalidOperationException("Database transaction have already opened");
            }

            _transaction = UnderlyingDbConnection.BeginTransaction(isolationLevel);
            return Task.FromResult(_transaction);
        }

        public async Task<IAsyncDisposable> Open(bool commit, CancellationToken token)
        {
            await Open(token).ConfigureAwait(false);
            return AsyncDisposable.Create((commit, token), state => Close(state.commit, state.token));
        }

        public async Task<IAsyncDisposable> Open(bool commit, IsolationLevel isolationLevel, CancellationToken token)
        {
            await Open(isolationLevel, token).ConfigureAwait(false);
            return AsyncDisposable.Create((commit, token), state => Close(state.commit, state.token));
        }

        public async Task Close(bool commit, CancellationToken token)
        {
            try
            {
                if (commit)
                {
                    await _changesTracker.SaveChanges(token).ConfigureAwait(false);
                    _transaction?.Commit();
                }
                else
                {
                    _transaction?.Rollback();
                    _transaction?.Dispose();
                }
            }
            finally
            {
                Interlocked.Exchange(ref _transaction, default)?.Dispose();
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
    }
}