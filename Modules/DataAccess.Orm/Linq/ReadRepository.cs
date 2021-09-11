namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Linq;
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Scoped)]
    internal class ReadRepository<TEntity, TKey> : IReadRepository<TEntity, TKey>
        where TEntity : IUniqueIdentified<TKey>
    {
        private readonly IAsyncQueryProvider _queryProvider;

        public ReadRepository(IAsyncQueryProvider queryProvider)
        {
            _queryProvider = queryProvider;
        }

        public IQueryable<TEntity> All()
        {
            return _queryProvider
                .CreateQuery<TEntity>(this.QueryAll());
        }

        public TEntity Single(TKey key)
        {
            return _queryProvider
                .CreateQuery<TEntity>(this.QuerySingle(key))
                .Single();
        }

        public TEntity? SingleOrDefault(TKey key)
        {
            return _queryProvider
                .CreateQuery<TEntity>(this.QuerySingleOrDefault(key))
                .SingleOrDefault();
        }
    }
}