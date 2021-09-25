namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Database
{
    using System.Data;
    using System.Linq;
    using Api.Model;
    using AutoRegistration.Api.Abstractions;
    using Connection;

    internal interface IInMemoryDatabase : IResolvable
    {
        IAdvancedDbTransaction BeginTransaction(InMemoryDbConnection connection, IsolationLevel isolationLevel);

        void EndTransaction(IAdvancedDbTransaction transaction);

        Entry Create<TEntity, TKey>(TEntity entity, IAdvancedDbTransaction transaction)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;

        Entry? Read<TEntity, TKey>(TKey primaryKey, IAdvancedDbTransaction transaction)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;

        IQueryable<TEntity> ReadAll<TEntity, TKey>(InMemoryDbTransaction transaction)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;

        Entry Update<TEntity, TKey>(TEntity entity, IAdvancedDbTransaction transaction)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;

        Entry Delete<TEntity, TKey>(TKey primaryKey, IAdvancedDbTransaction transaction)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;
    }
}