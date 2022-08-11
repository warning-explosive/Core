namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Persisting;
    using Api.Transaction;
    using Microsoft.Extensions.Logging;

    internal class CreateEntityChange : ITransactionalChange
    {
        private readonly IUniqueIdentified[] _entities;
        private readonly EnInsertBehavior _insertBehavior;

        public CreateEntityChange(
            IUniqueIdentified[] entities,
            EnInsertBehavior insertBehavior)
        {
            _entities = entities;
            _insertBehavior = insertBehavior;
        }

        public Task Apply(
            IAdvancedDatabaseTransaction databaseTransaction,
            ILogger logger,
            CancellationToken token)
        {
            return databaseTransaction
               .Write()
               .Insert(_entities, _insertBehavior, token);
        }

        public void Apply(ITransactionalStore transactionalStore)
        {
            foreach (var entity in _entities)
            {
                transactionalStore.Store(entity);
            }
        }
    }
}