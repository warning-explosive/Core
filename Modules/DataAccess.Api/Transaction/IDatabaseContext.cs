namespace SpaceEngineers.Core.DataAccess.Api.Transaction
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using GenericDomain.Api.Abstractions;
    using Model;
    using Persisting;
    using Reading;

    /// <summary>
    /// IDatabaseContext
    /// </summary>
    public interface IDatabaseContext : IResolvable
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
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;

        /// <summary>
        /// Gets access to IBulkRepository so as to produce bulk writes to database
        /// </summary>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>IBulkRepository</returns>
        IBulkRepository<TEntity, TKey> BulkWrite<TEntity, TKey>()
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull;

        /// <summary>
        /// Starts tracking specified aggregate root with underlying domain events
        /// </summary>
        /// <param name="aggregate">Aggregate root</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Track(IAggregate aggregate, CancellationToken token);
    }
}