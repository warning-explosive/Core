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
    /// <typeparam name="TEntity">TEntity type-argument</typeparam>
    public interface IReadRepository<TEntity>
        where TEntity : IUniqueIdentified
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
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Linq query</returns>
        [SuppressMessage("Analysis", "CA1716", Justification = "desired name")]
        [SuppressMessage("Analysis", "CA1720", Justification = "desired name")]
        public TEntity Single<TKey>(TKey key)
            where TKey : notnull;

        /// <summary>
        /// Retrieves an element by its primary key
        /// </summary>
        /// <param name="key">Primary key</param>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Linq query</returns>
        public TEntity? SingleOrDefault<TKey>(TKey key)
            where TKey : notnull;

        /// <summary>
        /// Retrieves an element by its primary key
        /// </summary>
        /// <param name="key">Primary key</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Linq query</returns>
        [SuppressMessage("Analysis", "CA1716", Justification = "desired name")]
        [SuppressMessage("Analysis", "CA1720", Justification = "desired name")]
        public Task<TEntity> SingleAsync<TKey>(TKey key, CancellationToken token)
            where TKey : notnull;

        /// <summary>
        /// Retrieves an element by its primary key
        /// </summary>
        /// <param name="key">Primary key</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Linq query</returns>
        public Task<TEntity?> SingleOrDefaultAsync<TKey>(TKey key, CancellationToken token)
            where TKey : notnull;
    }
}