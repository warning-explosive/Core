namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Model;
    using Orm.Linq;
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
            this IInsertQueryable<TEntity> source,
            CancellationToken token)
            where TEntity : IDatabaseEntity
        {
            var queryable = (Queryable<TEntity>)source;

            var (dependencyContainer, transaction, entities, insertBehavior) = InsertCommandExpressionVisitor.Extract(queryable.Expression);

            var modelProvider = dependencyContainer.Resolve<IModelProvider>();

            var version = await transaction
                .GetVersion(token)
                .ConfigureAwait(false);

            foreach (var entity in entities.SelectMany(modelProvider.Flatten).OfType<IDatabaseEntity>().ToList())
            {
                entity.Version = version;
            }

            var affectedRowsCount = await queryable
                .AsyncQueryProvider
                .ExecuteNonQueryAsync(queryable.Expression, token)
                .ConfigureAwait(false);

            transaction.CollectChange(new CreateEntityChange(entities, insertBehavior));

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
            this IFilteredUpdateQueryable<TEntity> source,
            CancellationToken token)
            where TEntity : IDatabaseEntity
        {
            var queryable = (Queryable<TEntity>)source;

            var (transaction, setExpressions, predicate) = UpdateCommandExpressionVisitor<TEntity>.Extract(queryable.Expression);

            var versions = (await ((ICachedQueryable<long>)transaction
                   .All<TEntity>()
                   .Where(predicate)
                   .Select(entity => entity.Version))
                   .ToListAsync(token)
                   .ConfigureAwait(false))
               .GroupBy(version => version)
               .ToDictionary(
                    grp => grp.Key,
                    grp => grp.Count());

            var updateVersion = await transaction
                .GetVersion(token)
                .ConfigureAwait(false);

            var affectedRowsCount = await queryable
                .AsyncQueryProvider
                .ExecuteNonQueryAsync(queryable.Expression, token)
                .ConfigureAwait(false);

            foreach (var (version, count) in versions)
            {
                transaction.CollectChange(new UpdateEntityChange<TEntity>(version, count, setExpressions, predicate, updateVersion));
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
            this IFilteredDeleteQueryable<TEntity> source,
            CancellationToken token)
            where TEntity : IDatabaseEntity
        {
            var queryable = (Queryable<TEntity>)source;

            var (transaction, predicate) = DeleteCommandExpressionVisitor<TEntity>.Extract(queryable.Expression);

            var versions = (await ((ICachedQueryable<long>)transaction
                        .All<TEntity>()
                        .Where(predicate)
                        .Select(entity => entity.Version))
                    .ToListAsync(token)
                    .ConfigureAwait(false))
                .GroupBy(version => version)
                .ToDictionary(
                    grp => grp.Key,
                    grp => grp.Count());

            var affectedRowsCount = await queryable
                .AsyncQueryProvider
                .ExecuteNonQueryAsync(queryable.Expression, token)
                .ConfigureAwait(false);

            foreach (var (version, count) in versions)
            {
                transaction.CollectChange(new DeleteEntityChange<TEntity>(version, count, predicate));
            }

            return affectedRowsCount;
        }

        #endregion
    }
}