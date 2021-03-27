namespace SpaceEngineers.Core.DataAccess.EntityFramework.Internals
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Contract.Abstractions;
    using GenericDomain.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class ReadRepository<TAggregate, TSpecification> : IReadRepository<TAggregate, TSpecification>
        where TAggregate : class, IAggregate
        where TSpecification : IReadRepositorySpecification
    {
        private readonly ApplicationDatabaseContext _databaseContext;
        private readonly IPredicateFactory<TAggregate, TSpecification> _predicateFactory;

        public ReadRepository(ApplicationDatabaseContext databaseContext,
                              IPredicateFactory<TAggregate, TSpecification> predicateFactory)
        {
            _databaseContext = databaseContext;
            _predicateFactory = predicateFactory;
        }
        
        public async Task<TAggregate> Read(TSpecification spec)
        {
            var predicate = await _predicateFactory.Build(spec).ConfigureAwait(false);

            return _databaseContext.Set<TAggregate>().Single(predicate);
        }
    }
}