namespace SpaceEngineers.Core.DataAccess.Contract.Abstractions
{
    using AutoWiring.Api.Abstractions;
    using GenericDomain.Abstractions;

    /// <summary>
    /// IReadRepository
    /// </summary>
    /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
    /// <typeparam name="TSpecification">TSpecification type-argument</typeparam>
    public interface IReadRepository<TAggregate, TSpecification> : IResolvable
        where TAggregate : class, IAggregate
        where TSpecification : IReadRepositorySpecification
    {
        /// <summary>
        /// Read aggregate from persistence layer
        /// </summary>
        /// <param name="spec">Specification</param>
        /// <returns>Ongoing read operation</returns>
        TAggregate Read(TSpecification spec);
    }
}