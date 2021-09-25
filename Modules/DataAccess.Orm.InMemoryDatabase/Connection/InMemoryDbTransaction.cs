namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Connection
{
    using System;
    using System.Collections.Concurrent;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Exceptions;
    using Api.Model;
    using Basics;
    using Database;

    internal class InMemoryDbTransaction : IAdvancedDbTransaction
    {
        private readonly IInMemoryDatabase _database;

        private int _isCompleted;

        public InMemoryDbTransaction(
            IInMemoryDatabase database,
            IDbConnection connection,
            IsolationLevel isolationLevel)
        {
            Timestamp = DateTime.UtcNow;
            Changes = new ConcurrentDictionary<Type, ConcurrentDictionary<object, ConcurrentStack<Entry>>>();
            _database = database;

            _isCompleted = 0;

            Id = Guid.NewGuid();
            Connection = connection;
            IsolationLevel = isolationLevel;
        }

        public Guid Id { get; }

        public DateTime Timestamp { get; }

        public ConcurrentDictionary<Type, ConcurrentDictionary<object, ConcurrentStack<Entry>>> Changes { get; }

        public IDbConnection Connection { get; }

        public IsolationLevel IsolationLevel { get; }

        public void Dispose()
        {
            Rollback();
        }

        public void Commit()
        {
            if (IsCompleted())
            {
                return;
            }

            Complete();
        }

        public void Rollback()
        {
            if (IsCompleted())
            {
                return;
            }

            Changes.Clear();

            Complete();
        }

        public IQueryable All(Type entityType)
        {
            return _database
                .CallMethod(nameof(_database.ReadAll))
                .WithTypeArgument(entityType)
                .WithTypeArgument(entityType.ExtractGenericArgumentsAt(typeof(IUniqueIdentified<>)).Single())
                .WithArgument(this)
                .Invoke<IQueryable>();
        }

        public Task Insert<TEntity, TKey>(TEntity entity, CancellationToken token)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            var entry = _database.Create<TEntity, TKey>(entity, this);
            return AddEntry(entry);
        }

        public Task<TEntity> Read<TEntity, TKey>(TKey primaryKey, CancellationToken token)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            var entity = _database.Read<TEntity, TKey>(primaryKey, this)?.Entity
                        ?? throw new EntityNotFoundException<TEntity, TKey>(primaryKey);

            return Task.FromResult((TEntity)entity);
        }

        public Task Update<TEntity, TKey, TValue>(TEntity entity, CancellationToken token)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            var entry = _database.Update<TEntity, TKey>(entity, this);
            return AddEntry(entry);
        }

        public Task Delete<TEntity, TKey>(TKey primaryKey, CancellationToken token)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            var entry = _database.Delete<TEntity, TKey>(primaryKey, this);
            return AddEntry(entry);
        }

        private Task AddEntry(Entry entry)
        {
            lock (Changes)
            {
                var collection = Changes.GetOrAdd(
                    entry.Type,
                    _ => new ConcurrentDictionary<object, ConcurrentStack<Entry>>());

                var versions = collection.GetOrAdd(
                    entry.PrimaryKey,
                    _ => new ConcurrentStack<Entry>());

                versions.Push(entry);

                return Task.CompletedTask;
            }
        }

        private bool IsCompleted()
        {
            return Interlocked.CompareExchange(ref _isCompleted, int.MinValue, int.MinValue) != 0;
        }

        private void Complete()
        {
            try
            {
                _database.EndTransaction(this);
            }
            finally
            {
                Interlocked.Exchange(ref _isCompleted, 1);
            }
        }
    }
}