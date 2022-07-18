namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Persisting;
    using Api.Transaction;

    internal class CreateEntityChange : ITransactionalChange
    {
        private readonly IUniqueIdentified[] _entities;
        private readonly EnInsertBehavior _insertBehavior;
        private readonly long _expectedAffectedRowsCount;

        public CreateEntityChange(
            IUniqueIdentified[] entities,
            EnInsertBehavior insertBehavior,
            long expectedAffectedRowsCount)
        {
            _entities = entities;
            _insertBehavior = insertBehavior;
            _expectedAffectedRowsCount = expectedAffectedRowsCount;
        }

        public Task Apply(
            IAdvancedDatabaseTransaction databaseTransaction,
            CancellationToken token)
        {
            return databaseTransaction
               .Write()
               .Insert(_entities, _insertBehavior, token);
        }
    }
}