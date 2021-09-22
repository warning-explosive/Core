namespace SpaceEngineers.Core.DataAccess.Api.Persisting
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using DatabaseEntity;

    /// <summary>
    /// IBulkRepository
    /// </summary>
    /// <typeparam name="TEntity">TEntity type-argument</typeparam>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public interface IBulkRepository<TEntity, TKey> : IResolvable
        where TEntity : IUniqueIdentified<TKey>
    {
        /// <summary>
        /// Inserts entity in the database
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Insert(IEnumerable<TEntity> entity, CancellationToken token);

        /// <summary>
        /// Updates entity in the database
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="accessor">Field accessor</param>
        /// <param name="value">Value</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        Task Update<TValue>(IEnumerable<TEntity> entity, Func<TEntity, TValue> accessor, TValue value, CancellationToken token);

        /// <summary>
        /// Updates entity in the database
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="accessor">Field accessor</param>
        /// <param name="valueProducer">Value producer</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        Task Update<TValue>(IEnumerable<TEntity> entity, Func<TEntity, TValue> accessor, Func<TEntity, TValue> valueProducer, CancellationToken token);

        /// <summary>
        /// Updates entity in the database
        /// </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="accessor">Field accessor</param>
        /// <param name="value">Value</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        Task Update<TValue>(IEnumerable<TKey> primaryKey, Func<TEntity, TValue> accessor, TValue value, CancellationToken token);

        /// <summary>
        /// Updates entity in the database
        /// </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="accessor">Field accessor</param>
        /// <param name="valueProducer">Value producer</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        Task Update<TValue>(IEnumerable<TKey> primaryKey, Func<TEntity, TValue> accessor, Func<TEntity, TValue> valueProducer, CancellationToken token);

        /// <summary>
        /// Deletes entity from the database
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Delete(IEnumerable<TEntity> entity, CancellationToken token);

        /// <summary>
        /// Deletes entity from the database
        /// </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Delete(IEnumerable<TKey> primaryKey, CancellationToken token);
    }
}