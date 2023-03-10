namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Primitives;
    using Connection;
    using Linq;
    using Model;
    using Translation;

    [Component(EnLifestyle.Scoped)]
    internal class DatabaseTransaction : IAdvancedDatabaseTransaction,
                                         IDisposable,
                                         IResolvable<IAdvancedDatabaseTransaction>,
                                         IResolvable<IDatabaseTransaction>,
                                         IResolvable<IDatabaseContext>
    {
        private readonly IDatabaseConnectionProvider _connectionProvider;
        private readonly IRepository _repository;

        private readonly List<ITransactionalChange> _changes;
        private readonly List<ICommand> _commands;

        [SuppressMessage("Analysis", "CA2213", Justification = "disposed with Interlocked.Exchange")]
        private IDbConnection? _connection;

        [SuppressMessage("Analysis", "CA2213", Justification = "disposed with Interlocked.Exchange")]
        private IDbTransaction? _transaction;

        private long? _version;

        public DatabaseTransaction(
            IDatabaseConnectionProvider connectionProvider,
            IRepository repository,
            ITransactionalStore transactionalStore)
        {
            _connectionProvider = connectionProvider;
            _repository = repository;

            Store = transactionalStore;

            _changes = new List<ITransactionalChange>();
            _commands = new List<ICommand>();
        }

        #region IAdvancedDatabaseTransaction

        public IDbTransaction DbTransaction => _transaction ?? throw new InvalidOperationException("Transaction should be opened before any interactions with it");

        public IDbConnection DbConnection
        {
            get
            {
                for (var i = 0; i < 3; i++)
                {
                    switch (_connection?.State)
                    {
                        case ConnectionState.Connecting:
                            try
                            {
                                Task.Delay(TimeSpan.FromMilliseconds(50)).Wait();
                            }
                            catch (OperationCanceledException)
                            {
                                break;
                            }

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

        public bool HasChanges => _changes.Any();

        public IReadOnlyCollection<ICommand> Commands => _commands;

        public bool Connected => _connection != null;

        public ITransactionalStore Store { get; }

        public void CollectChange(ITransactionalChange change)
        {
            _changes.Add(change);
            Store.Apply(change);
        }

        public void CollectCommand(ICommand command)
        {
            _commands.Add(command);
        }

        public async Task<long> GetVersion(CancellationToken token)
        {
            _version ??= await _connectionProvider
                .GetVersion(this, token)
                .ConfigureAwait(false);

            return _version.Value;
        }

        #endregion

        #region IDatabaseContext

        public IQueryable<TEntity> All<TEntity>()
            where TEntity : IUniqueIdentified
        {
            return _repository.All<TEntity>();
        }

        public Task<TEntity> Single<TEntity, TKey>(TKey key, CancellationToken token)
            where TEntity : IUniqueIdentified
            where TKey : notnull
        {
            return _repository.Single<TEntity, TKey>(key, token);
        }

        public Task<TEntity?> SingleOrDefault<TEntity, TKey>(TKey key, CancellationToken token)
            where TEntity : IUniqueIdentified
            where TKey : notnull
        {
            return _repository.SingleOrDefault<TEntity, TKey>(key, token);
        }

        public IInsertQueryable<IDatabaseEntity> Insert(
            IReadOnlyCollection<IDatabaseEntity> entities,
            EnInsertBehavior insertBehavior)
        {
            return _repository.Insert(this, entities, insertBehavior);
        }

        public IUpdateQueryable<TEntity> Update<TEntity>()
            where TEntity : IDatabaseEntity
        {
            return _repository.Update<TEntity>(this);
        }

        public IDeleteQueryable<TEntity> Delete<TEntity>()
            where TEntity : IDatabaseEntity
        {
            return _repository.Delete<TEntity>(this);
        }

        #endregion

        #region IDatabaseTransaction

        public async Task<IAsyncDisposable> OpenScope(bool commit, CancellationToken token)
        {
            await Open(token).ConfigureAwait(false);

            return AsyncDisposable.Create((commit, token), state => Close(state.commit, state.token));
        }

        public async Task Open(CancellationToken token)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Database transaction have already been opened");
            }

            _transaction = await _connectionProvider
                .BeginTransaction(DbConnection, token)
                .ConfigureAwait(false);
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
                _commands.Clear();
                Store.Clear();

                Interlocked.Exchange(ref _transaction, default)?.Dispose();
                Interlocked.Exchange(ref _connection, default)?.Dispose();
            }

            if (!changes.Any())
            {
                return;
            }

            try
            {
                await Open(token).ConfigureAwait(false);

                foreach (var change in changes)
                {
                    await change
                       .Apply(this, token)
                       .ConfigureAwait(false);
                }

                _transaction.Commit();
            }
            finally
            {
                _changes.Clear();
                _commands.Clear();
                Store.Clear();

                Interlocked.Exchange(ref _transaction, default)?.Dispose();
                Interlocked.Exchange(ref _connection, default)?.Dispose();
            }
        }

        #endregion

        #region IDissposable

        public void Dispose()
        {
            Close(false, CancellationToken.None).Wait();
        }

        #endregion
    }
}