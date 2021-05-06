namespace SpaceEngineers.Core.DataAccess.Contract.Abstractions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IReadRepository
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface IReadRepository<T> : IResolvable
    {
        /// <summary>
        /// Read aggregate from persistence layer
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing read operation</returns>
        Task<IEnumerable<T>> Read(IQueryable<T> query, CancellationToken token);
    }
}