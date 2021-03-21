namespace SpaceEngineers.Core.DataAccess.Contract.Abstractions
{
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
        void Create(TAggregate aggregate);

        /// <summary>
        /// Update aggregate in the persistence layer
        /// </summary>
        /// <param name="aggregate">Aggregate object instance</param>
        void Update(TAggregate aggregate);

        /// <summary>
        /// Delete aggregate from the persistence layer
        /// </summary>
        /// <param name="aggregate">Aggregate object instance</param>
        void Delete(TAggregate aggregate);
    }
}