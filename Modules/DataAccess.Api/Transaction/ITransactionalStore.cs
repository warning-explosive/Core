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
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        void Store<TEntity, TKey>(TEntity entity)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;

        /// <summary>
        /// Gets entities from transactional store by predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Request result</returns>
        IEnumerable<TEntity> GetValues<TEntity, TKey>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;

        /// <summary>
        /// Gets entity from transactional store by key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="entity">Entity</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Request result</returns>
        bool TryGetValue<TEntity, TKey>(TKey key, [NotNullWhen(true)] out TEntity? entity)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;

        /// <summary>
        /// Tries to remove specified entity from transactional store
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="entity">Removed entity</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Removal result</returns>
        bool TryRemove<TEntity, TKey>(TKey key, [NotNullWhen(true)] out TEntity? entity)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;

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