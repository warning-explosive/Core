namespace SpaceEngineers.Core.DataAccess.Contract.Abstractions
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;
    using GenericDomain.Abstractions;

    /// <summary>
    /// IBulkRepository
    /// </summary>
    /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
    public interface IBulkRepository<TAggregate> : IResolvable
        where TAggregate : class, IAggregate
    {
        /// <summary>
        /// Create aggregates in the persistence layer
        /// </summary>
        /// <param name="aggregate">Aggregate object instances</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing create operation</returns>
        Task Create(IEnumerable<TAggregate> aggregate, CancellationToken token);

        /// <summary>
        /// Update aggregates in the persistence layer
        /// </summary>
        /// <param name="aggregate">Aggregate object instances</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing update operation</returns>
        Task Update(IEnumerable<TAggregate> aggregate, CancellationToken token);

        /// <summary>
        /// Delete aggregates from the persistence layer
        /// </summary>
        /// <param name="aggregate">Aggregate object instances</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing delete operation</returns>
        Task Delete(IEnumerable<TAggregate> aggregate, CancellationToken token);
    }
}