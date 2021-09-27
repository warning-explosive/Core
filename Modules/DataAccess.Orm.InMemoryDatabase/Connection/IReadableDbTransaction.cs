namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Connection
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;

    internal interface IReadableDbTransaction
    {
        IQueryable All(Type type);

        Task<TEntity> Single<TEntity, TKey>(TKey primaryKey, CancellationToken token)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;
    }
}