namespace SpaceEngineers.Core.DataAccess.Api.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Model;
    using Persisting;
    using Reading;

    /// <summary>
    /// IDatabaseContext
    /// </summary>
    public interface IDatabaseContext
    {
        /// <summary>
        /// Gets access to IReadRepository so as to produce reads from database
        /// </summary>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>IReadRepository</returns>
        IReadRepository<TEntity> Read<TEntity>()
            where TEntity : IUniqueIdentified;

        /// <summary>
        /// Inserts entities in the database
        /// </summary>
        /// <param name="entities">Entities</param>
        /// <param name="insertBehavior">EnInsertBehavior</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Affected rows count</returns>
        Task<long> Insert(
            IDatabaseEntity[] entities,
            EnInsertBehavior insertBehavior,
            CancellationToken token);

        /// <summary>
        /// Inserts entity in the database
        /// </summary>
        /// <param name="entities">Entities</param>
        /// <param name="insertBehavior">EnInsertBehavior</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Affected rows count</returns>
        Task<long> Insert<TEntity>(
            IReadOnlyCollection<TEntity> entities,
            EnInsertBehavior insertBehavior,
            CancellationToken token)
            where TEntity : IDatabaseEntity;

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