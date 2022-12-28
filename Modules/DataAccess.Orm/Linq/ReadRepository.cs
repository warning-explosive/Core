namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Reading;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class ReadRepository<TEntity> : IReadRepository<TEntity>,
                                             IResolvable<IReadRepository<TEntity>>
        where TEntity : IUniqueIdentified
    {
        private static readonly MethodInfo LinqAll = LinqMethods.All(typeof(TEntity));

        private readonly IAsyncQueryProvider _queryProvider;

        public ReadRepository(IAsyncQueryProvider queryProvider)
        {
            _queryProvider = queryProvider;
        }

        public IQueryable<TEntity> All()
        {
            return _queryProvider.CreateQuery<TEntity>(QueryAll(this));
        }

        public TEntity Single<TKey>(TKey key)
            where TKey : notnull
        {
            return SingleByPrimaryKey(key).Single();
        }

        public TEntity? SingleOrDefault<TKey>(TKey key)
            where TKey : notnull
        {
            return SingleByPrimaryKey(key).SingleOrDefault();
        }

        public Task<TEntity> SingleAsync<TKey>(TKey key, CancellationToken token)
            where TKey : notnull
        {
            return SingleByPrimaryKey(key).SingleAsync(token);
        }

        public Task<TEntity?> SingleOrDefaultAsync<TKey>(TKey key, CancellationToken token)
            where TKey : notnull
        {
            return SingleByPrimaryKey(key).SingleOrDefaultAsync(token);
        }

        private static Expression QueryAll(IReadRepository<TEntity> readRepository)
        {
            return Expression.Call(
                Expression.Constant(readRepository),
                LinqAll);
        }

        private IQueryable<TEntity> SingleByPrimaryKey<TKey>(TKey key)
            where TKey : notnull
        {
            var expression = Expression.Call(
                null,
                LinqMethods.QueryableWhere().MakeGenericMethod(typeof(TEntity)),
                QueryAll(this),
                Predicate(key));

            return _queryProvider.CreateQuery<TEntity>(expression);

            static Expression<Func<TEntity, bool>> Predicate(TKey key)
            {
                return entity => Equals(entity.PrimaryKey, key);
            }
        }
    }
}