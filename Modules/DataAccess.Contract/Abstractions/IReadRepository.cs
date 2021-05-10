namespace SpaceEngineers.Core.DataAccess.Contract.Abstractions
{
    using System;
    using System.Linq;
    using AutoWiring.Api.Abstractions;
    using GenericDomain.Abstractions;

    /// <summary>
    /// IReadRepository
    /// </summary>
    /// <typeparam name="TEntity">TEntity type-argument</typeparam>
    public interface IReadRepository<TEntity> : IResolvable
        where TEntity : class, IEntity
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
        /// <returns>Linq query</returns>
        public TEntity Single(Guid key);

        /// <summary>
        /// Retrieves an element by its primary key
        /// </summary>
        /// <param name="key">Primary key</param>
        /// <returns>Linq query</returns>
        public TEntity? SingleOrDefault(Guid key);
    }
}