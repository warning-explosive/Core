namespace SpaceEngineers.Core.DataAccess.EntityFramework.Internals
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Contract.Abstractions;
    using GenericDomain.Abstractions;

    [Lifestyle(EnLifestyle.Scoped)]
    internal class Repository<TAggregate> : IRepository<TAggregate>
        where TAggregate : class, IAggregate
    {
        private readonly ApplicationDatabaseContext _databaseContext;

        public Repository(ApplicationDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        
        public void Create(TAggregate aggregate)
        {
            _databaseContext.Set<TAggregate>().Add(aggregate);
        }

        public void Update(TAggregate aggregate)
        {
            _databaseContext.Set<TAggregate>().Update(aggregate);
        }

        public void Delete(TAggregate aggregate)
        {
            _databaseContext.Set<TAggregate>().Remove(aggregate);
        }
    }
}