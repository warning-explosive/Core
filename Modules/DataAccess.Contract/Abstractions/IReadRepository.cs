namespace SpaceEngineers.Core.DataAccess.Contract.Abstractions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;
    using GenericDomain.Abstractions;

    /// <summary>
    /// IReadRepository
    /// </summary>
    /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
    public interface IReadRepository<TAggregate> : IResolvable
        where TAggregate : class, IAggregate
    {
        /// <summary>
        /// Read aggregate from persistence layer
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns>Ongoing read operation</returns>
        Task<IEnumerable<TAggregate>> ReadAll(IQueryable<TAggregate> query);
    }
}