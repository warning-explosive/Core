namespace SpaceEngineers.Core.DataAccess.Api.Reading
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Model;

    /// <summary>
    /// IReadRepository
    /// </summary>
    public interface IReadRepository
    {
        /// <summary>
        /// Creates entry point for every linq query
        /// </summary>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Linq query</returns>
        public IQueryable<TEntity> All<TEntity>()
            where TEntity : IUniqueIdentified;

        /// <summary>
        /// Retrieves an element by its primary key
        /// </summary>
        /// <param name="key">Primary key</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Linq query</returns>
        [SuppressMessage("Analysis", "CA1716", Justification = "desired name")]
        [SuppressMessage("Analysis", "CA1720", Justification = "desired name")]
        public Task<TEntity> Single<TEntity, TKey>(TKey key, CancellationToken token)
            where TEntity : IUniqueIdentified
            where TKey : notnull;

        /// <summary>
        /// Retrieves an element by its primary key
        /// </summary>
        /// <param name="key">Primary key</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Linq query</returns>
        public Task<TEntity?> SingleOrDefault<TEntity, TKey>(TKey key, CancellationToken token)
            where TEntity : IUniqueIdentified
            where TKey : notnull;
    }
}