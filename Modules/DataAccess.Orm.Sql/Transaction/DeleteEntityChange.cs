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
        private readonly long _version;
        private readonly long _affectedRowsCount;
        private readonly Expression<Func<TEntity, bool>> _predicate;

        /// <summary> .cctor </summary>
        /// <param name="version">Version</param>
        /// <param name="affectedRowsCount">Affected rows count</param>
        /// <param name="predicate">Predicate</param>
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

        /// <inheritdoc />
        public async Task Apply(
            IAdvancedDatabaseTransaction transaction,
            CancellationToken token)
        {
            var actualAffectedRowsCount = await transaction
               .Delete<TEntity>()
               .Where(Predicate)
               /* TODO: .CachedExpression("D584BA2A-EC1D-420B-8B99-40F58A03A595")*/
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
            var values = transactionalStore.GetValues(Predicate);

            foreach (var entity in values)
            {
                transactionalStore.TryRemove<TEntity>(entity.PrimaryKey, out _);
            }
        }
    }
}