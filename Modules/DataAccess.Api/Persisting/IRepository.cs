namespace SpaceEngineers.Core.DataAccess.Api.Persisting
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Model;

    /// <summary>
    /// IRepository
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Inserts entities in the database
        /// </summary>
        /// <param name="entities">Entities</param>
        /// <param name="insertBehavior">EnInsertBehavior</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Affected rows count</returns>
        Task<long> Insert(
            IUniqueIdentified[] entities,
            EnInsertBehavior insertBehavior,
            CancellationToken token);
    }

    /// <summary>
    /// IRepository
    /// </summary>
    /// <typeparam name="TEntity">TEntity type-argument</typeparam>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public interface IRepository<TEntity, TKey>
        where TEntity : IDatabaseEntity<TKey>
        where TKey : notnull
    {
        /// <summary>
        /// Inserts entity in the database
        /// </summary>
        /// <param name="entities">Entities</param>
        /// <param name="insertBehavior">EnInsertBehavior</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Affected rows count</returns>
        Task<long> Insert(
            IReadOnlyCollection<TEntity> entities,
            EnInsertBehavior insertBehavior,
            CancellationToken token);

        /// <summary>
        /// Updates entity in the database
        /// </summary>
        /// <param name="accessor">Field accessor</param>
        /// <param name="valueProducer">Value producer</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        /// <returns>Affected rows count</returns>
        Task<long> Update<TValue>(
            Expression<Func<TEntity, TValue>> accessor,
            Expression<Func<TEntity, TValue>> valueProducer,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token);

        /// <summary>
        /// Deletes entity from the database
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Affected rows count</returns>
        Task<long> Delete(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token);
    }
}