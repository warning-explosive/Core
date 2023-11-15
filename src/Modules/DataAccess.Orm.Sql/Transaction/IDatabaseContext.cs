namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Transaction
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Linq;
    using Model;

    /// <summary>
    /// IDatabaseContext
    /// </summary>
    public interface IDatabaseContext
    {
        /// <summary>
        /// Creates entry point for every linq query
        /// </summary>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Linq query</returns>
        public IQueryable<TEntity> All<TEntity>()
            where TEntity : IUniqueIdentified;

        /// <summary>
        /// Retrieves an element by its primary key
        /// </summary>
        /// <param name="key">Primary key</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Linq query</returns>
        [SuppressMessage("Analysis", "CA1716", Justification = "desired name")]
        [SuppressMessage("Analysis", "CA1720", Justification = "desired name")]
        public Task<TEntity> Single<TEntity, TKey>(TKey key, CancellationToken token)
            where TEntity : IDatabaseEntity<TKey>
            where TKey : notnull;

        /// <summary>
        /// Retrieves an element by its primary key
        /// </summary>
        /// <param name="key">Primary key</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Linq query</returns>
        public Task<TEntity?> SingleOrDefault<TEntity, TKey>(TKey key, CancellationToken token)
            where TEntity : IDatabaseEntity<TKey>
            where TKey : notnull;

        /// <summary>
        /// Inserts entities in the database
        /// </summary>
        /// <param name="entities">Entities</param>
        /// <param name="insertBehavior">EnInsertBehavior</param>
        /// <returns>Affected rows count</returns>
        IInsertQueryable<IDatabaseEntity> Insert(
            IReadOnlyCollection<IDatabaseEntity> entities,
            EnInsertBehavior insertBehavior);

        /// <summary>
        /// Updates entity in the database
        /// </summary>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Affected rows count</returns>
        IUpdateQueryable<TEntity> Update<TEntity>()
            where TEntity : IDatabaseEntity;

        /// <summary>
        /// Deletes entity from the database
        /// </summary>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Affected rows count</returns>
        IDeleteQueryable<TEntity> Delete<TEntity>()
            where TEntity : IDatabaseEntity;
    }
}