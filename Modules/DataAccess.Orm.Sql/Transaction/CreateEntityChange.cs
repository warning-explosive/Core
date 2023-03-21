namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Linq;
    using Model;

    /// <summary>
    /// CreateEntityChange
    /// </summary>
    public class CreateEntityChange : ITransactionalChange
    {
        private readonly IReadOnlyCollection<IDatabaseEntity> _entities;
        private readonly EnInsertBehavior _insertBehavior;
        private readonly string _cacheKey;

        /// <summary> .cctor </summary>
        /// <param name="entities">Entities</param>
        /// <param name="insertBehavior">Insert behavior</param>
        /// <param name="cacheKey">Cache key</param>
        public CreateEntityChange(
            IReadOnlyCollection<IDatabaseEntity> entities,
            EnInsertBehavior insertBehavior,
            string cacheKey)
        {
            if (!entities.Any())
            {
                throw new InvalidOperationException("Entities are empty");
            }

            _entities = entities;
            _insertBehavior = insertBehavior;
            _cacheKey = cacheKey;
        }

        /// <inheritdoc />
        public Task Apply(
            IAdvancedDatabaseTransaction transaction,
            CancellationToken token)
        {
            return transaction
                .Insert(_entities, _insertBehavior)
                .CachedExpression(_cacheKey)
                .Invoke(token);
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