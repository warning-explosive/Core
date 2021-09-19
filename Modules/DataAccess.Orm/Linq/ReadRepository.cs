namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.DatabaseEntity;
    using Api.Reading;
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

        public Task<TEntity> SingleAsync(TKey key, CancellationToken token)
        {
            return _queryProvider
                .CreateQuery<TEntity>(this.QuerySingle(key))
                .SingleAsync(token);
        }

        public Task<TEntity?> SingleOrDefaultAsync(TKey key, CancellationToken token)
        {
            return _queryProvider
                .CreateQuery<TEntity>(this.QuerySingleOrDefault(key))
                .SingleOrDefaultAsync(token);
        }
    }
}