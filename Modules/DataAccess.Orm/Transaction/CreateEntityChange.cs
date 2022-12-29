namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Persisting;
    using Api.Transaction;

    /// <summary>
    /// CreateEntityChange
    /// </summary>
    public class CreateEntityChange : ITransactionalChange
    {
        private readonly IReadOnlyCollection<IDatabaseEntity> _entities;
        private readonly EnInsertBehavior _insertBehavior;

        /// <summary> .cctor </summary>
        /// <param name="entities">Entities</param>
        /// <param name="insertBehavior">Insert behavior</param>
        public CreateEntityChange(
            IReadOnlyCollection<IDatabaseEntity> entities,
            EnInsertBehavior insertBehavior)
        {
            _entities = entities;
            _insertBehavior = insertBehavior;
        }

        /// <inheritdoc />
        public Task Apply(
            IAdvancedDatabaseTransaction databaseTransaction,
            CancellationToken token)
        {
            return databaseTransaction.Insert(_entities, _insertBehavior, token);
        }

        /// <inheritdoc />
        public void Apply(ITransactionalStore transactionalStore)
        {
            foreach (var entity in _entities)
            {
                transactionalStore.Store(entity);
            }
        }
    }
}