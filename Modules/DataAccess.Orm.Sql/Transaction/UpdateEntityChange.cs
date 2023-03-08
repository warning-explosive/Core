namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Transaction
{
    using System;
    using System.Collections.Generic;
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
        private readonly long _affectedRowsCount;

        private readonly Expression<Action<TEntity>> _setVersion;

        private readonly IReadOnlyCollection<Expression<Action<TEntity>>> _setExpressions;

        private readonly IReadOnlyCollection<Action<TEntity>> _setFuncs;

        private readonly Expression<Func<TEntity, bool>> _predicate;

        private readonly string _cacheKey;

        /// <summary> .cctor </summary>
        /// <param name="version">Version</param>
        /// <param name="updateVersion">Update version</param>
        /// <param name="affectedRowsCount">Affected rows count</param>
        /// <param name="setExpressions">Set expression</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cacheKey">Cache key</param>
        public UpdateEntityChange(
            long version,
            long updateVersion,
            long affectedRowsCount,
            IReadOnlyCollection<Expression<Action<TEntity>>> setExpressions,
            Expression<Func<TEntity, bool>> predicate,
            string cacheKey)
        {
            _affectedRowsCount = affectedRowsCount;

            _setVersion = entity => entity.Version.Assign(updateVersion);

            _setExpressions = setExpressions;

            var setFuncs = new List<Action<TEntity>>(setExpressions.Count + 1) { SetFunc(_setVersion) };

            foreach (var setExpression in setExpressions)
            {
                setFuncs.Add(SetFunc(setExpression));
            }

            _setFuncs = setFuncs;

            _predicate = predicate.And(entity => entity.Version == version);

            _cacheKey = cacheKey;
        }

        /// <inheritdoc />
        public async Task Apply(
            IAdvancedDatabaseTransaction transaction,
            CancellationToken token)
        {
            var updateSource = transaction
                .Update<TEntity>()
                .Set(_setVersion);

            foreach (var setExpression in _setExpressions)
            {
                updateSource = updateSource.Set(setExpression);
            }

            var actualAffectedRowsCount = await updateSource
                .Where(_predicate)
                .CachedExpression($"{nameof(UpdateEntityChange<TEntity>)}:{_cacheKey}")
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
            var entities = transactionalStore.GetValues(_predicate);

            foreach (var entity in entities)
            {
                foreach (var setFunc in _setFuncs)
                {
                    setFunc(entity);
                }
            }
        }

        private static Action<TEntity> SetFunc(Expression<Action<TEntity>> setExpression)
        {
            return ReplaceAssignExpressionVisitor.Replace(setExpression).Compile();
        }
    }
}