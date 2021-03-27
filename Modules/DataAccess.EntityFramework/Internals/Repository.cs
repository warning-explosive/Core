namespace SpaceEngineers.Core.DataAccess.EntityFramework.Internals
{
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Contract.Abstractions;
    using GenericDomain.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class Repository<TAggregate> : IRepository<TAggregate>
        where TAggregate : class, IAggregate
    {
        private readonly ApplicationDatabaseContext _databaseContext;

        public Repository(ApplicationDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        
        public Task Create(TAggregate aggregate)
        {
            return _databaseContext.Set<TAggregate>().AddAsync(aggregate).AsTask();
        }

        public Task Update(TAggregate aggregate)
        {
            _databaseContext.Set<TAggregate>().Update(aggregate);

            return Task.CompletedTask;
        }

        public Task Delete(TAggregate aggregate)
        {
            _databaseContext.Set<TAggregate>().Remove(aggregate);

            return Task.CompletedTask;
        }
    }
}