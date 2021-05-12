namespace SpaceEngineers.Core.DataAccess.Orm.Internals
{
    using System;
    using System.Linq;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Contract.Abstractions;
    using GenericDomain.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class ReadRepository<TEntity> : IReadRepository<TEntity>
        where TEntity : class, IEntity
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

        public TEntity Single(Guid key)
        {
            return _queryProvider
                .CreateQuery<TEntity>(this.QuerySingle(key))
                .Single();
        }

        public TEntity? SingleOrDefault(Guid key)
        {
            return _queryProvider
                .CreateQuery<TEntity>(this.QuerySingleOrDefault(key))
                .SingleOrDefault();
        }
    }
}