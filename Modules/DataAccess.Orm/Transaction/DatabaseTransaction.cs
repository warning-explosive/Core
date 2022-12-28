namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
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
    using CompositionRoot;
    using Connection;

    [Component(EnLifestyle.Scoped)]
    internal class DatabaseTransaction : IAdvancedDatabaseTransaction,
                                         IDisposable,
                                         IResolvable<IAdvancedDatabaseTransaction>,
                                         IResolvable<IDatabaseTransaction>,
                                         IResolvable<IDatabaseContext>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseConnectionProvider _connectionProvider;
        private readonly IRepository _repository;

        private readonly List<ITransactionalChange> _changes;

        [SuppressMessage("Analysis", "CA2213", Justification = "disposed with Interlocked.Exchange")]
        private IDatabaseConnection? _connection;

        [SuppressMessage("Analysis", "CA2213", Justification = "disposed with Interlocked.Exchange")]
        private IDbTransaction? _transaction;

        public DatabaseTransaction(
            IDependencyContainer dependencyContainer,
            IDatabaseConnectionProvider connectionProvider,
            IRepository repository,
            ITransactionalStore transactionalStore)
        {
            _dependencyContainer = dependencyContainer;
            _connectionProvider = connectionProvider;
            _repository = repository;

            Store = transactionalStore;
            _changes = new List<ITransactionalChange>();
        }

        public bool HasChanges => _changes.Any();

        public IDbTransaction DbTransaction => _transaction ?? throw new InvalidOperationException("Transaction should be opened before any interactions with it");

        public IDatabaseConnection DbConnection
        {
            get
            {
                for (var i = 0; i < 3; i++)
                {
                    switch (_connection?.UnderlyingDbConnection.State)
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

        public bool Connected => _connection?.UnderlyingDbConnection != null;

        public ITransactionalStore Store { get; }

        public string? LastCommand { get; private set; }

        public IReadRepository<TEntity> Read<TEntity>()
            where TEntity : IUniqueIdentified
        {
            return _dependencyContainer.Resolve<IReadRepository<TEntity>>();
        }

        public Task<long> Insert(
            IDatabaseEntity[] entities,
            EnInsertBehavior insertBehavior,
            CancellationToken token)
        {
            return _repository.Insert(this, entities, insertBehavior, token);
        }

        public Task<long> Insert<TEntity>(
            IReadOnlyCollection<TEntity> entities,
            EnInsertBehavior insertBehavior,
            CancellationToken token)
            where TEntity : IDatabaseEntity
        {
            return _dependencyContainer
                .Resolve<IRepository<TEntity>>()
                .Insert(this, entities, insertBehavior, token);
        }

        public Task<long> Update<TEntity, TValue>(
            Expression<Func<TEntity, TValue>> accessor,
            Expression<Func<TEntity, TValue>> valueProducer,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token)
            where TEntity : IDatabaseEntity
        {
            return _dependencyContainer
                .Resolve<IRepository<TEntity>>()
                .Update(this, accessor, valueProducer, predicate, token);
        }

        public Task<long> Update<TEntity>(
            IReadOnlyCollection<UpdateInfo<TEntity>> infos,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token)
            where TEntity : IDatabaseEntity
        {
            return _dependencyContainer
                .Resolve<IRepository<TEntity>>()
                .Update(this, infos, predicate, token);
        }

        public Task<long> Delete<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token)
            where TEntity : IDatabaseEntity
        {
            return _dependencyContainer
                .Resolve<IRepository<TEntity>>()
                .Delete(this, predicate, token);
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
                _transaction = Open();

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
                Store.Clear();

                Interlocked.Exchange(ref _transaction, default)?.Dispose();
                Interlocked.Exchange(ref _connection, default)?.Dispose();
            }
        }

        public void Dispose()
        {
            Close(false, CancellationToken.None).Wait();
        }

        public void CollectChange(ITransactionalChange change)
        {
            _changes.Add(change);
            Store.Apply(change);
        }

        public void CollectCommand(string commandText)
        {
            LastCommand = commandText;
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