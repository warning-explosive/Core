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
        private readonly IAsyncQueryProvider<TEntity> _queryProvider;

        public ReadRepository(IAsyncQueryProvider<TEntity> queryProvider)
        {
            _queryProvider = queryProvider;
        }

        public IQueryable<TEntity> All()
        {
            var expresion = RootQueries.QueryAll(typeof(TEntity));
            return _queryProvider.CreateQuery(expresion);
        }

        public TEntity Single(Guid key)
        {
            var expression = RootQueries.QuerySingle(typeof(TEntity), key);
            return _queryProvider.CreateQuery(expression).Single();
        }

        public TEntity? SingleOrDefault(Guid key)
        {
            var expression = RootQueries.QuerySingleOrDefault(typeof(TEntity), key);
            return _queryProvider.CreateQuery(expression).SingleOrDefault();
        }
    }
}