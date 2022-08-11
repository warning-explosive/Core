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
    using Microsoft.Extensions.Logging;

    internal class DeleteEntityChange<TEntity> : ITransactionalChange
        where TEntity : IDatabaseEntity
    {
        private readonly long _version;
        private readonly long _affectedRowsCount;
        private readonly Expression<Func<TEntity, bool>> _predicate;

        public DeleteEntityChange(
            long version,
            long affectedRowsCount,
            Expression<Func<TEntity, bool>> predicate)
        {
            _version = version;
            _affectedRowsCount = affectedRowsCount;
            _predicate = predicate;
        }

        private Expression<Func<TEntity, bool>> Predicate => _predicate.And(entity => entity.Version == _version);

        public async Task Apply(
            IAdvancedDatabaseTransaction databaseTransaction,
            ILogger logger,
            CancellationToken token)
        {
            var actualAffectedRowsCount = await databaseTransaction
               .Write<TEntity>()
               .Delete(Predicate, token)
               .ConfigureAwait(false);

            if (actualAffectedRowsCount != _affectedRowsCount)
            {
                throw new DatabaseConcurrentUpdateException(typeof(TEntity));
            }
        }

        public void Apply(ITransactionalStore transactionalStore)
        {
            var values = transactionalStore.GetValues(Predicate);

            foreach (var entity in values)
            {
                transactionalStore.TryRemove<TEntity>(entity.PrimaryKey, out _);
            }
        }
    }
}