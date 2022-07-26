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

    internal class DeleteEntityChange<TEntity, TKey> : ITransactionalChange
        where TEntity : IDatabaseEntity<TKey>
        where TKey : notnull
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

        public async Task Apply(
            IAdvancedDatabaseTransaction databaseTransaction,
            ILogger logger,
            CancellationToken token)
        {
            var actualAffectedRowsCount = await databaseTransaction
               .Write<TEntity, TKey>()
               .Delete(_predicate.And(entity => entity.Version == _version), token)
               .ConfigureAwait(false);

            if (actualAffectedRowsCount != _affectedRowsCount)
            {
                throw new DatabaseConcurrentUpdateException(typeof(TEntity));
            }
        }
    }
}