namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    /// UpdateEntityChange
    /// </summary>
    /// <typeparam name="TEntity">TEntity type-argument</typeparam>
    public class UpdateEntityChange<TEntity> : ITransactionalChange
        where TEntity : IDatabaseEntity
    {
        private readonly long _version;
        private readonly long _affectedRowsCount;
        private readonly IReadOnlyCollection<Expression<Action<TEntity>>> _setExpressions;
        private readonly Expression<Func<TEntity, bool>> _predicate;
        private readonly long _updateVersion;

        /// <summary> .cctor </summary>
        /// <param name="version">Version</param>
        /// <param name="affectedRowsCount">Affected rows count</param>
        /// <param name="setExpressions">Set expression</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="updateVersion">Update version</param>
        public UpdateEntityChange(
            long version,
            long affectedRowsCount,
            IReadOnlyCollection<Expression<Action<TEntity>>> setExpressions,
            Expression<Func<TEntity, bool>> predicate,
            long updateVersion)
        {
            _version = version;
            _affectedRowsCount = affectedRowsCount;
            _setExpressions = setExpressions;
            _predicate = predicate;
            _updateVersion = updateVersion;
        }

        private Expression<Func<TEntity, bool>> Predicate => _predicate.And(entity => entity.Version == _version);

        /// <inheritdoc />
        public async Task Apply(
            IAdvancedDatabaseTransaction transaction,
            CancellationToken token)
        {
            Expression<Action<TEntity>> setVersion = entity => entity.Version.Assign(_updateVersion);

            var updateSource = transaction
                .Update<TEntity>()
                .Set(setVersion);

            foreach (var setExpression in _setExpressions)
            {
                updateSource = updateSource.Set(setExpression);
            }

            var actualAffectedRowsCount = await updateSource
                .Where(Predicate)
                /* TODO: .CachedExpression("87E76EC1-A31A-43F5-A3D2-80FCF49AAA71")*/
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
            Expression<Action<TEntity>> setVersion = entity => entity.Version.Assign(_updateVersion);

            var entities = transactionalStore.GetValues(Predicate);

            foreach (var entity in entities)
            {
                foreach (var setExpression in _setExpressions.Concat(new[] { setVersion }))
                {
                    ReplaceAssignExpressionVisitor
                        .Replace(setExpression)
                        .Compile()
                        .Invoke(entity);
                }
            }
        }
    }
}