namespace SpaceEngineers.Core.DataAccess.Contract.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;
    using GenericDomain.Abstractions;

    /// <summary>
    /// IRepository
    /// </summary>
    /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
    public interface IRepository<TAggregate> : IResolvable
        where TAggregate : class, IAggregate
    {
        /// <summary>
        /// Create aggregate in the persistence layer
        /// </summary>
        /// <param name="aggregate">Aggregate object instance</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing create operation</returns>
        Task Create(TAggregate aggregate, CancellationToken token);

        /// <summary>
        /// Update aggregate in the persistence layer
        /// </summary>
        /// <param name="aggregate">Aggregate object instance</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing update operation</returns>
        Task Update(TAggregate aggregate, CancellationToken token);

        /// <summary>
        /// Delete aggregate from the persistence layer
        /// </summary>
        /// <param name="aggregate">Aggregate object instance</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing delete operation</returns>
        Task Delete(TAggregate aggregate, CancellationToken token);
    }
}