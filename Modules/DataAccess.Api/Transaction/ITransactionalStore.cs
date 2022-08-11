namespace SpaceEngineers.Core.DataAccess.Api.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using Model;

    /// <summary>
    /// Transactional store
    /// </summary>
    public interface ITransactionalStore : IDisposable
    {
        /// <summary>
        /// Puts entity into transactional store
        /// </summary>
        /// <param name="entity">Entity</param>
        void Store(IUniqueIdentified entity);

        /// <summary>
        /// Gets entities from transactional store by predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Request result</returns>
        IEnumerable<TEntity> GetValues<TEntity>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : IUniqueIdentified;

        /// <summary>
        /// Gets entity from transactional store by key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="entity">Entity</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Request result</returns>
        bool TryGetValue<TEntity>(object key, [NotNullWhen(true)] out TEntity? entity)
            where TEntity : IUniqueIdentified;

        /// <summary>
        /// Tries to remove specified entity from transactional store
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="entity">Removed entity</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Removal result</returns>
        bool TryRemove<TEntity>(object key, [NotNullWhen(true)] out TEntity? entity)
            where TEntity : IUniqueIdentified;

        /// <summary>
        /// Clears transactional store
        /// </summary>
        void Clear();

        /// <summary>
        /// Applies change to transactional store
        /// </summary>
        /// <param name="change">change</param>
        void Apply(ITransactionalChange change);
    }
}