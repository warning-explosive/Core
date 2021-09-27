namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Database
{
    using Api.Model;
    using AutoRegistration.Api.Abstractions;
    using Connection;

    internal interface IWritableDatabase : IResolvable
    {
        Entry Create<TEntity, TKey>(TEntity entity, IAdvancedDbTransaction transaction)
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