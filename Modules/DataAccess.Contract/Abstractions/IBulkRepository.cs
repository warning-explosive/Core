namespace SpaceEngineers.Core.DataAccess.Contract.Abstractions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;
    using GenericDomain.Abstractions;

    /// <summary>
    /// IBulkRepository
    /// </summary>
    /// <typeparam name="TAggregate"></typeparam>
    public interface IBulkRepository<TAggregate> : IResolvable
        where TAggregate : class, IAggregate
    {
        /// <summary>
        /// Create aggregates in the persistence layer
        /// </summary>
        /// <param name="aggregate">Aggregate object instances</param>
        Task Create(IEnumerable<TAggregate> aggregate);

        /// <summary>
        /// Update aggregates in the persistence layer
        /// </summary>
        /// <param name="aggregate">Aggregate object instances</param>
        Task Update(IEnumerable<TAggregate> aggregate);

        /// <summary>
        /// Delete aggregates from the persistence layer
        /// </summary>
        /// <param name="aggregate">Aggregate object instances</param>
        Task Delete(IEnumerable<TAggregate> aggregate);
    }
}