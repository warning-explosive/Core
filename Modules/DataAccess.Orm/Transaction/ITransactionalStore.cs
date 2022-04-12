namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Transactional store
    /// </summary>
    public interface ITransactionalStore : IDisposable
    {
        /// <summary>
        /// Puts entry into transactional store
        /// </summary>
        /// <param name="obj">Object entry</param>
        /// <param name="keySelector">Key selector</param>
        /// <typeparam name="TEntry">TEntry type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        void Store<TEntry, TKey>(
            TEntry obj,
            Func<TEntry, TKey> keySelector)
            where TEntry : notnull
            where TKey : notnull;

        /// <summary>
        /// Gets entry from transactional store by key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="entry">Entry</param>
        /// <typeparam name="TEntry">TEntry type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Request result</returns>
        bool TryGetValue<TEntry, TKey>(TKey key, [NotNullWhen(true)] out TEntry? entry)
            where TEntry : notnull
            where TKey : notnull;
    }
}