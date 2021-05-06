namespace SpaceEngineers.Core.DataAccess.Orm.Internals
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Contract.Abstractions;
    using GenericDomain.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class Repository<TAggregate> : IRepository<TAggregate>
        where TAggregate : class, IAggregate
    {
        private readonly IBulkRepository<TAggregate> _bulkRepository;

        public Repository(IBulkRepository<TAggregate> bulkRepository)
        {
            _bulkRepository = bulkRepository;
        }

        public Task Create(TAggregate aggregate, CancellationToken token)
        {
            return _bulkRepository.Create(new[] { aggregate }, token);
        }

        public Task Update(TAggregate aggregate, CancellationToken token)
        {
            return _bulkRepository.Update(new[] { aggregate }, token);
        }

        public Task Delete(TAggregate aggregate, CancellationToken token)
        {
            return _bulkRepository.Delete(new[] { aggregate }, token);
        }
    }
}