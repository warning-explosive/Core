namespace SpaceEngineers.Core.DataAccess.Api.Abstractions
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IReadRepository
    /// </summary>
    /// <typeparam name="TEntity">TEntity type-argument</typeparam>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public interface IReadRepository<TEntity, TKey> : IResolvable
        where TEntity : IUniqueIdentified<TKey>
    {
        /// <summary>
        /// Creates entry point for every linq query
        /// </summary>
        /// <returns>Linq query</returns>
        public IQueryable<TEntity> All();

        /// <summary>
        /// Retrieves an element by its primary key
        /// </summary>
        /// <param name="key">Primary key</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Linq query</returns>
        [SuppressMessage("Analysis", "CA1716", Justification = "desired name")]
        [SuppressMessage("Analysis", "CA1720", Justification = "desired name")]
        public Task<TEntity> SingleAsync(TKey key, CancellationToken token);

        /// <summary>
        /// Retrieves an element by its primary key
        /// </summary>
        /// <param name="key">Primary key</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Linq query</returns>
        public Task<TEntity?> SingleOrDefaultAsync(TKey key, CancellationToken token);
    }
}