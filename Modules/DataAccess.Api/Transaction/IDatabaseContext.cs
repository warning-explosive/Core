namespace SpaceEngineers.Core.DataAccess.Api.Transaction
{
    using Model;
    using Persisting;
    using Reading;

    /// <summary>
    /// IDatabaseContext
    /// </summary>
    public interface IDatabaseContext
    {
        /// <summary>
        /// Are there any changes in the database transaction
        /// </summary>
        bool HasChanges { get; }

        /// <summary>
        /// Gets access to IReadRepository so as to produce reads from database
        /// </summary>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>IReadRepository</returns>
        IReadRepository<TEntity, TKey> Read<TEntity, TKey>()
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;

        /// <summary>
        /// Gets access to IRepository so as to produce writes to database
        /// </summary>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>IRepository</returns>
        IRepository<TEntity, TKey> Write<TEntity, TKey>()
            where TEntity : IDatabaseEntity<TKey>
            where TKey : notnull;

        /// <summary>
        /// Gets access to IRepository so as to produce writes to database
        /// </summary>
        /// <returns>IRepository</returns>
        IRepository Write();
    }
}