namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Exceptions;
    using Api.Model;
    using Api.Transaction;
    using Basics;

    internal class DeleteEntityChange<TEntity, TKey> : ITransactionalChange
        where TEntity : IDatabaseEntity<TKey>
        where TKey : notnull
    {
        private readonly Expression<Func<TEntity, bool>> _predicate;
        private readonly long _version;
        private readonly long _expectedAffectedRowsCount;

        public DeleteEntityChange(
            Expression<Func<TEntity, bool>> predicate,
            long version,
            long expectedAffectedRowsCount)
        {
            _predicate = predicate;
            _version = version;
            _expectedAffectedRowsCount = expectedAffectedRowsCount;
        }

        public async Task Apply(
            IAdvancedDatabaseTransaction databaseTransaction,
            CancellationToken token)
        {
            var actualAffectedRowsCount = await databaseTransaction
               .Write<TEntity, TKey>()
               .Delete(_predicate.And(entity => entity.Version < _version), token)
               .ConfigureAwait(false);

            if (actualAffectedRowsCount != _expectedAffectedRowsCount)
            {
                throw new DatabaseConcurrentUpdateException(typeof(TEntity));
            }
        }
    }
}