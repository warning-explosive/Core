namespace SpaceEngineers.Core.DataAccess.Api.Transaction
{
    using System.Diagnostics.CodeAnalysis;
    using Model;

    /// <summary>
    /// IAdvancedDatabaseTransaction
    /// </summary>
    public interface IAdvancedDatabaseTransaction : IDatabaseTransaction
    {
        /// <summary>
        /// Puts entry into transactional store
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        void Store<TEntity, TKey>(TEntity entity)
            where TEntity : IDatabaseEntity<TKey>
            where TKey : notnull;

        /// <summary>
        /// Gets entry from transactional store by key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="entity">Entity</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Request result</returns>
        bool TryGetValue<TEntity, TKey>(TKey key, [NotNullWhen(true)] out TEntity? entity)
            where TEntity : IDatabaseEntity<TKey>
            where TKey : notnull;
    }
}