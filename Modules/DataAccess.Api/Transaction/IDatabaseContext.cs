namespace SpaceEngineers.Core.DataAccess.Api.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Model;
    using Persisting;

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
            where TEntity : IUniqueIdentified
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
            where TEntity : IUniqueIdentified
            where TKey : notnull;

        /// <summary>
        /// Inserts entities in the database
        /// </summary>
        /// <param name="entities">Entities</param>
        /// <param name="insertBehavior">EnInsertBehavior</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Affected rows count</returns>
        Task<long> Insert(
            IReadOnlyCollection<IDatabaseEntity> entities,
            EnInsertBehavior insertBehavior,
            CancellationToken token);

        /// <summary>
        /// Updates entity in the database
        /// </summary>
        /// <param name="accessor">Field accessor</param>
        /// <param name="valueProducer">Value producer</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        /// <returns>Affected rows count</returns>
        Task<long> Update<TEntity, TValue>(
            Expression<Func<TEntity, TValue>> accessor,
            Expression<Func<TEntity, TValue>> valueProducer,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token)
            where TEntity : IDatabaseEntity;

        /// <summary>
        /// Updates entity in the database
        /// </summary>
        /// <param name="infos">Update infos</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Affected rows count</returns>
        Task<long> Update<TEntity>(
            IReadOnlyCollection<UpdateInfo<TEntity>> infos,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token)
            where TEntity : IDatabaseEntity;

        /// <summary>
        /// Deletes entity from the database
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Affected rows count</returns>
        Task<long> Delete<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token)
            where TEntity : IDatabaseEntity;
    }
}