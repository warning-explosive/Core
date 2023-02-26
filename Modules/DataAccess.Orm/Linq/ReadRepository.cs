namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Reading;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class ReadRepository : IReadRepository,
                                    IResolvable<IReadRepository>
    {
        private readonly IAsyncQueryProvider _queryProvider;

        public ReadRepository(IAsyncQueryProvider queryProvider)
        {
            _queryProvider = queryProvider;
        }

        public IQueryable<TEntity> All<TEntity>()
            where TEntity : IUniqueIdentified
        {
            return _queryProvider.CreateQuery<TEntity>(QueryAll<TEntity>(this));
        }

        public Task<TEntity> Single<TEntity, TKey>(TKey key, CancellationToken token)
            where TEntity : IUniqueIdentified
            where TKey : notnull
        {
            return SingleByPrimaryKey<TEntity, TKey>(key)
                .CachedExpression("E9FF5F3C-13C0-4269-9BAE-7064E8681EBE")
                .SingleAsync(token);
        }

        public Task<TEntity?> SingleOrDefault<TEntity, TKey>(TKey key, CancellationToken token)
            where TEntity : IUniqueIdentified
            where TKey : notnull
        {
            return SingleByPrimaryKey<TEntity, TKey>(key)
                .CachedExpression("C73644BD-4C73-43CC-984D-462CB39DADA2")
                .SingleOrDefaultAsync(token);
        }

        private static Expression QueryAll<TEntity>(IReadRepository readRepository)
        {
            return Expression.Call(
                Expression.Constant(readRepository),
                LinqMethods.RepositoryAll().MakeGenericMethod(typeof(TEntity)));
        }

        private IQueryable<TEntity> SingleByPrimaryKey<TEntity, TKey>(TKey key)
            where TEntity : IUniqueIdentified
            where TKey : notnull
        {
            var expression = Expression.Call(
                null,
                LinqMethods.QueryableWhere().MakeGenericMethod(typeof(TEntity)),
                QueryAll<TEntity>(this),
                Predicate(key));

            return _queryProvider.CreateQuery<TEntity>(expression);

            static Expression<Func<TEntity, bool>> Predicate(TKey key)
            {
                return entity => Equals(entity.PrimaryKey, key);
            }
        }
    }
}