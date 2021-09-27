namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Database
{
    using System.Linq;
    using Api.Model;
    using AutoRegistration.Api.Abstractions;
    using Connection;

    internal interface IReadableDatabase : IResolvable
    {
        IQueryable<TEntity> All<TEntity, TKey>(InMemoryDbTransaction transaction)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;

        Entry? SingleOrDefault<TEntity, TKey>(TKey primaryKey, IAdvancedDbTransaction transaction)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;
    }
}