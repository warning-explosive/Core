namespace SpaceEngineers.Core.DataAccess.Api.Persisting
{
    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Model;

    /// <summary>
    /// IBulkRepository
    /// </summary>
    /// <typeparam name="TEntity">TEntity type-argument</typeparam>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public interface IBulkRepository<TEntity, TKey>
        where TEntity : IUniqueIdentified<TKey>
        where TKey : notnull
    {
        /// <summary>
        /// Inserts entity in the database
        /// </summary>
        /// <param name="entities">Entities</param>
        /// <param name="insertBehavior">EnInsertBehavior</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Insert(TEntity[] entities, EnInsertBehavior insertBehavior, CancellationToken token);

        /// <summary>
        /// Updates entity in the database
        /// </summary>
        /// <param name="primaryKeys">Primary keys</param>
        /// <param name="accessor">Field accessor</param>
        /// <param name="value">Value</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        Task Update<TValue>(TKey[] primaryKeys, Expression<Func<TEntity, TValue>> accessor, TValue value, CancellationToken token);

        /// <summary>
        /// Updates entity in the database
        /// </summary>
        /// <param name="primaryKeys">Primary keys</param>
        /// <param name="accessor">Field accessor</param>
        /// <param name="valueProducer">Value producer</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        Task Update<TValue>(TKey[] primaryKeys, Expression<Func<TEntity, TValue>> accessor, Expression<Func<TEntity, TValue>> valueProducer, CancellationToken token);

        /// <summary>
        /// Deletes entity from the database
        /// </summary>
        /// <param name="primaryKeys">Primary keys</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Delete(TKey[] primaryKeys, CancellationToken token);
    }
}