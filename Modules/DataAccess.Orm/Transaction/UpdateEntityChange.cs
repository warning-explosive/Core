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

    internal class UpdateEntityChange<TEntity, TKey, TValue> : ITransactionalChange
        where TEntity : IDatabaseEntity<TKey>
        where TKey : notnull
    {
        private readonly long _version;
        private readonly long _expectedAffectedRowsCount;
        private readonly Expression<Func<TEntity, TValue>> _accessor;
        private readonly Expression<Func<TEntity, TValue>> _valueProducer;
        private readonly Expression<Func<TEntity, bool>> _predicate;

        public UpdateEntityChange(
            long version,
            long expectedAffectedRowsCount,
            Expression<Func<TEntity, TValue>> accessor,
            Expression<Func<TEntity, TValue>> valueProducer,
            Expression<Func<TEntity, bool>> predicate)
        {
            _version = version;
            _expectedAffectedRowsCount = expectedAffectedRowsCount;
            _accessor = accessor;
            _valueProducer = valueProducer;
            _predicate = predicate;
        }

        public async Task Apply(
            IAdvancedDatabaseTransaction databaseTransaction,
            CancellationToken token)
        {
            var actualAffectedRowsCount = await databaseTransaction
               .Write<TEntity, TKey>()
               .Update(_accessor, _valueProducer, _predicate.And(entity => entity.Version < _version), token)
               .ConfigureAwait(false);

            if (actualAffectedRowsCount != _expectedAffectedRowsCount)
            {
                throw new DbConcurrentUpdateException(typeof(TEntity));
            }
        }
    }
}