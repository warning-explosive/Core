namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Persisting
{
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Connection;

    internal static class DatabaseTransactionExtensions
    {
        internal static Task Insert<TEntity, TKey>(this IDbTransaction transaction, TEntity entity, CancellationToken token)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            return ((IAdvancedDbTransaction)transaction).Insert<TEntity, TKey>(entity, token);
        }

        internal static Task<TEntity> Single<TEntity, TKey>(this IDbTransaction transaction, TKey primaryKey, CancellationToken token)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            return ((IAdvancedDbTransaction)transaction).Single<TEntity, TKey>(primaryKey, token);
        }

        internal static Task Update<TEntity, TKey, TValue>(this IDbTransaction transaction, TEntity entity, CancellationToken token)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            return ((IAdvancedDbTransaction)transaction).Update<TEntity, TKey, TValue>(entity, token);
        }

        internal static Task Delete<TEntity, TKey>(this IDbTransaction transaction, TKey primaryKey, CancellationToken token)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            return ((IAdvancedDbTransaction)transaction).Delete<TEntity, TKey>(primaryKey, token);
        }
    }
}