namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Connection
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;

    internal interface IWritableDbTransaction
    {
        Task Insert<TEntity, TKey>(TEntity entity, CancellationToken token)
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