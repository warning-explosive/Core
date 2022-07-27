namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Persisting;
    using Api.Transaction;
    using Basics;
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
                var type = entity.GetType();

                _ = GetType()
                   .CallMethod(nameof(Store))
                   .WithTypeArguments(type, type.ExtractGenericArgumentAt(typeof(IUniqueIdentified<>)))
                   .WithArguments(transactionalStore, entity)
                   .Invoke();
            }
        }

        private static void Store<TEntity, TKey>(
            ITransactionalStore transactionalStore,
            TEntity entity)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            transactionalStore.Store<TEntity, TKey>(entity);
        }
    }
}