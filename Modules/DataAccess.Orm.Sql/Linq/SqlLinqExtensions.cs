namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Model;
    using Orm.Linq;
    using Orm.Transaction;
    using SpaceEngineers.Core.DataAccess.Api.Model;
    using Transaction;

    /// <summary>
    /// SqlLinqExtensions
    /// </summary>
    public static class SqlLinqExtensions
    {
        #region IRepository.Insert

        /// <summary>
        /// Executes insert query
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Affected rows count</returns>
        public static async Task<long> Invoke<TEntity>(
            this ICachedInsertQueryable<TEntity> source,
            CancellationToken token)
            where TEntity : IDatabaseEntity
        {
            var queryable = (Queryable<TEntity>)source;

            var (dependencyContainer, transaction, entities, insertBehavior, cacheKey) = InsertCommandExpressionVisitor.Extract(queryable.Expression);

            var modelProvider = dependencyContainer.Resolve<IModelProvider>();

            var version = await transaction
                .GetVersion(token)
                .ConfigureAwait(false);

            foreach (var entity in entities.SelectMany(modelProvider.Flatten).OfType<IDatabaseEntity>())
            {
                entity.Version = version;
            }

            var affectedRowsCount = await queryable
                .AsyncQueryProvider
                .ExecuteNonQueryAsync(queryable.Expression, token)
                .ConfigureAwait(false);

            transaction.CollectChange(new CreateEntityChange(entities, insertBehavior, cacheKey));

            return affectedRowsCount;
        }

        #endregion

        #region IRepository.Update

        /// <summary>
        /// Executes update query
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Affected rows count</returns>
        public static async Task<long> Invoke<TEntity>(
            this ICachedUpdateQueryable<TEntity> source,
            CancellationToken token)
            where TEntity : IDatabaseEntity
        {
            var queryable = (Queryable<TEntity>)source;

            var (transaction, setExpressions, predicate, cacheKey) = UpdateCommandExpressionVisitor<TEntity>.Extract(queryable.Expression);

            var versions = await GetVersions(transaction, predicate, cacheKey, token).ConfigureAwait(false);

            var updateVersion = await transaction
                .GetVersion(token)
                .ConfigureAwait(false);

            var affectedRowsCount = await queryable
                .AsyncQueryProvider
                .ExecuteNonQueryAsync(queryable.Expression, token)
                .ConfigureAwait(false);

            foreach (var (version, count) in versions)
            {
                transaction.CollectChange(new UpdateEntityChange<TEntity>(version, updateVersion, count, setExpressions, predicate, cacheKey));
            }

            return affectedRowsCount;
        }

        #endregion

        #region IRepository.Delete

        /// <summary>
        /// Executes delete query
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Affected rows count</returns>
        public static async Task<long> Invoke<TEntity>(
            this ICachedDeleteQueryable<TEntity> source,
            CancellationToken token)
            where TEntity : IDatabaseEntity
        {
            var queryable = (Queryable<TEntity>)source;

            var (transaction, predicate, cacheKey) = DeleteCommandExpressionVisitor<TEntity>.Extract(queryable.Expression);

            var versions = await GetVersions(transaction, predicate, cacheKey, token).ConfigureAwait(false);

            var affectedRowsCount = await queryable
                .AsyncQueryProvider
                .ExecuteNonQueryAsync(queryable.Expression, token)
                .ConfigureAwait(false);

            foreach (var (version, count) in versions)
            {
                transaction.CollectChange(new DeleteEntityChange<TEntity>(version, count, predicate, cacheKey));
            }

            return affectedRowsCount;
        }

        private static async Task<Dictionary<long, int>> GetVersions<TEntity>(
            IAdvancedDatabaseTransaction transaction,
            Expression<Func<TEntity, bool>> predicate,
            string cacheKey,
            CancellationToken token)
            where TEntity : IDatabaseEntity
        {
            return (await transaction
                    .All<TEntity>()
                    .Where(predicate)
                    .Select(entity => entity.Version)
                    .CachedExpression($"{nameof(GetVersions)}:{cacheKey}")
                    .ToListAsync(token)
                    .ConfigureAwait(false))
                .GroupBy(version => version)
                .ToDictionary(
                    grp => grp.Key,
                    grp => grp.Count());
        }

        #endregion
    }
}