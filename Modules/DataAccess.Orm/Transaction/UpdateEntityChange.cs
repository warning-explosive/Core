namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Exceptions;
    using Api.Model;
    using Api.Persisting;
    using Api.Transaction;
    using Basics;

    /// <summary>
    /// UpdateEntityChange
    /// </summary>
    /// <typeparam name="TEntity">TEntity type-argument</typeparam>
    public class UpdateEntityChange<TEntity> : ITransactionalChange
        where TEntity : IDatabaseEntity
    {
        private readonly long _version;
        private readonly long _affectedRowsCount;
        private readonly IReadOnlyCollection<UpdateInfo<TEntity>> _infos;
        private readonly Expression<Func<TEntity, bool>> _predicate;
        private readonly long _updateVersion;

        /// <summary> .cctor </summary>
        /// <param name="version">Version</param>
        /// <param name="affectedRowsCount">Affected rows count</param>
        /// <param name="infos">Update infos</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="updateVersion">Update version</param>
        public UpdateEntityChange(
            long version,
            long affectedRowsCount,
            IReadOnlyCollection<UpdateInfo<TEntity>> infos,
            Expression<Func<TEntity, bool>> predicate,
            long updateVersion)
        {
            _version = version;
            _affectedRowsCount = affectedRowsCount;
            _infos = infos;
            _predicate = predicate;
            _updateVersion = updateVersion;
        }

        private IReadOnlyCollection<UpdateInfo<TEntity>> UpdateInfos => _infos
           .Concat(new[] { new UpdateInfo<TEntity>(entity => entity.Version, _ => _updateVersion) })
           .ToList();

        private Expression<Func<TEntity, bool>> Predicate => _predicate.And(entity => entity.Version == _version);

        /// <inheritdoc />
        public async Task Apply(
            IAdvancedDatabaseTransaction databaseTransaction,
            CancellationToken token)
        {
            var actualAffectedRowsCount = await databaseTransaction
               .Write<TEntity>()
               .Update(UpdateInfos, Predicate, token)
               .ConfigureAwait(false);

            if (actualAffectedRowsCount != _affectedRowsCount)
            {
                throw new DatabaseConcurrentUpdateException(typeof(TEntity));
            }
        }

        /// <inheritdoc />
        public void Apply(ITransactionalStore transactionalStore)
        {
            var entities = transactionalStore.GetValues(Predicate);

            foreach (var entity in entities)
            {
                foreach (var info in UpdateInfos)
                {
                    var parameter = info.Accessor.Parameters.Single();

                    var body = Expression.Assign(
                        info.Accessor.Body.UnwrapUnaryExpression().ReplaceParameter(parameter),
                        info.ValueProducer.Body.UnwrapUnaryExpression().ReplaceParameter(parameter));

                    Expression.Lambda<Action<TEntity>>(body, parameter)
                       .Compile()
                       .Invoke(entity);
                }
            }
        }
    }
}