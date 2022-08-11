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
        /// <returns>IReadRepository</returns>
        IReadRepository<TEntity> Read<TEntity>()
            where TEntity : IUniqueIdentified;

        /// <summary>
        /// Gets access to IRepository so as to produce writes to database
        /// </summary>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>IRepository</returns>
        IRepository<TEntity> Write<TEntity>()
            where TEntity : IDatabaseEntity;

        /// <summary>
        /// Gets access to IRepository so as to produce writes to database
        /// </summary>
        /// <returns>IRepository</returns>
        IRepository Write();
    }
}