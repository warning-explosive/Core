namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Reading;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Scoped)]
    internal class ReadRepository<TEntity, TKey> : IReadRepository<TEntity, TKey>
        where TEntity : IUniqueIdentified<TKey>
        where TKey : notnull
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
                .CreateQuery<TEntity>(this.QuerySingleAsync(key))
                .SingleAsync(token);
        }

        public Task<TEntity?> SingleOrDefaultAsync(TKey key, CancellationToken token)
        {
            return _queryProvider
                .CreateQuery<TEntity>(this.QuerySingleOrDefaultAsync(key))
                .SingleOrDefaultAsync(token);
        }
    }
}