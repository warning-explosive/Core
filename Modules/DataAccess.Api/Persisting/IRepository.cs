namespace SpaceEngineers.Core.DataAccess.Api.Persisting
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Model;
    using Transaction;

    /// <summary>
    /// IRepository
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Inserts entities in the database
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="entities">Entities</param>
        /// <param name="insertBehavior">EnInsertBehavior</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Affected rows count</returns>
        Task<long> Insert(
            IAdvancedDatabaseTransaction transaction,
            IReadOnlyCollection<IDatabaseEntity> entities,
            EnInsertBehavior insertBehavior,
            CancellationToken token);

        /// <summary>
        /// Updates entity in the database
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="accessor">Field accessor</param>
        /// <param name="valueProducer">Value producer</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        /// <returns>Affected rows count</returns>
        Task<long> Update<TEntity, TValue>(
            IAdvancedDatabaseTransaction transaction,
            Expression<Func<TEntity, TValue>> accessor,
            Expression<Func<TEntity, TValue>> valueProducer,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token)
            where TEntity : IDatabaseEntity;

        /// <summary>
        /// Updates entity in the database
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="infos">Update infos</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Affected rows count</returns>
        Task<long> Update<TEntity>(
            IAdvancedDatabaseTransaction transaction,
            IReadOnlyCollection<UpdateInfo<TEntity>> infos,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token)
            where TEntity : IDatabaseEntity;

        /// <summary>
        /// Deletes entity from the database
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Affected rows count</returns>
        Task<long> Delete<TEntity>(
            IAdvancedDatabaseTransaction transaction,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token)
            where TEntity : IDatabaseEntity;
    }
}