namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Connection
{
    using System;
    using System.Collections.Concurrent;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Database;

    internal interface IAdvancedDbTransaction : IDbTransaction
    {
        Guid Id { get; }

        DateTime Timestamp { get; }

        ConcurrentDictionary<Type, ConcurrentDictionary<object, ConcurrentStack<Entry>>> Changes { get; }

        IQueryable All(Type type);

        Task Insert<TEntity, TKey>(TEntity entity, CancellationToken token)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;

        Task<TEntity> Read<TEntity, TKey>(TKey primaryKey, CancellationToken token)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;

        Task Update<TEntity, TKey, TValue>(TEntity entity, CancellationToken token)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;

        Task Delete<TEntity, TKey>(TKey primaryKey, CancellationToken token)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;
    }
}