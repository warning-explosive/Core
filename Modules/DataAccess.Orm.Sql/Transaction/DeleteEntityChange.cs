namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Transaction
{
    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Exceptions;
    using Basics;
    using Linq;
    using Orm.Transaction;
    using SpaceEngineers.Core.DataAccess.Api.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Linq;

    /// <summary>
    /// DeleteEntityChange
    /// </summary>
    /// <typeparam name="TEntity">TEntity type-argument</typeparam>
    public class DeleteEntityChange<TEntity> : ITransactionalChange
        where TEntity : IDatabaseEntity
    {
        private readonly long _affectedRowsCount;
        private readonly Expression<Func<TEntity, bool>> _predicate;
        private readonly string _cacheKey;

        /// <summary> .cctor </summary>
        /// <param name="version">Version</param>
        /// <param name="affectedRowsCount">Affected rows count</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cacheKey">Cache key</param>
        public DeleteEntityChange(
            long version,
            long affectedRowsCount,
            Expression<Func<TEntity, bool>> predicate,
            string cacheKey)
        {
            _affectedRowsCount = affectedRowsCount;

            _predicate = predicate.And(entity => entity.Version == version);

            _cacheKey = cacheKey;
        }

        /// <inheritdoc />
        public async Task Apply(
            IAdvancedDatabaseTransaction transaction,
            CancellationToken token)
        {
            var actualAffectedRowsCount = await transaction
               .Delete<TEntity>()
               .Where(_predicate)
               .CachedExpression($"{nameof(DeleteEntityChange<TEntity>)}:{_cacheKey}")
               .Invoke(token)
               .ConfigureAwait(false);

            if (actualAffectedRowsCount != _affectedRowsCount)
            {
                throw new DatabaseConcurrentUpdateException(typeof(TEntity));
            }
        }

        /// <inheritdoc />
        public void Apply(ITransactionalStore transactionalStore)
        {
            var values = transactionalStore.GetValues(_predicate);

            foreach (var entity in values)
            {
                transactionalStore.TryRemove<TEntity>(entity.PrimaryKey, out _);
            }
        }
    }
}