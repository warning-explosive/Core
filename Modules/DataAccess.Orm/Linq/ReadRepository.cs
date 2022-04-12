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

    [Component(EnLifestyle.Scoped)]
    internal class ReadRepository<TEntity, TKey> : IReadRepository<TEntity, TKey>,
                                                   IResolvable<IReadRepository<TEntity, TKey>>
        where TEntity : IUniqueIdentified<TKey>
        where TKey : notnull
    {
        private static readonly MethodInfo LinqAll = LinqMethods.All(typeof(TEntity), typeof(TKey));
        private static readonly MethodInfo LinqWhere = LinqMethods.QueryableWhere();

        private readonly IAsyncQueryProvider _queryProvider;

        public ReadRepository(IAsyncQueryProvider queryProvider)
        {
            _queryProvider = queryProvider;
        }

        public IQueryable<TEntity> All()
        {
            return _queryProvider.CreateQuery<TEntity>(QueryAll(this));
        }

        public TEntity Single(TKey key)
        {
            return SingleByPrimaryKey(key).Single();
        }

        public TEntity? SingleOrDefault(TKey key)
        {
            return SingleByPrimaryKey(key).SingleOrDefault();
        }

        public Task<TEntity> SingleAsync(TKey key, CancellationToken token)
        {
            return SingleByPrimaryKey(key).SingleAsync(token);
        }

        public Task<TEntity?> SingleOrDefaultAsync(TKey key, CancellationToken token)
        {
            return SingleByPrimaryKey(key).SingleOrDefaultAsync(token);
        }

        private static Expression QueryAll(IReadRepository<TEntity, TKey> readRepository)
        {
            return Expression.Call(
                Expression.Constant(readRepository),
                LinqAll);
        }

        private IQueryable<TEntity> SingleByPrimaryKey(TKey key)
        {
            var expression = Expression.Call(
                null,
                LinqWhere.MakeGenericMethod(typeof(TEntity)),
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