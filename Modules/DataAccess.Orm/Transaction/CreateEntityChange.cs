namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Persisting;
    using Api.Transaction;

    internal class CreateEntityChange<TEntity, TKey> : ITransactionalChange
        where TEntity : IUniqueIdentified<TKey>
        where TKey : notnull
    {
        private readonly TEntity _entity;
        private readonly EnInsertBehavior _insertBehavior;

        public CreateEntityChange(
            TEntity entity,
            EnInsertBehavior insertBehavior)
        {
            _entity = entity;
            _insertBehavior = insertBehavior;
        }

        public Task Apply(IDatabaseContext databaseContext, CancellationToken token)
        {
            return databaseContext
               .Write<TEntity, TKey>()
               .Insert(new[] { _entity }, _insertBehavior, token);
        }
    }
}