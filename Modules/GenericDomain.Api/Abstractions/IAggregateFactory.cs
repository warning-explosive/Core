namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IAggregateFactory
    /// </summary>
    /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
    /// <typeparam name="TSpec">TSpec type-argument</typeparam>
    public interface IAggregateFactory<TAggregate, TSpec>
        where TAggregate : IAggregate
        where TSpec : IAggregateSpecification
    {
        /// <summary>
        /// Builds aggregate root
        /// </summary>
        /// <param name="spec">Specification</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Built aggregate root</returns>
        Task<TAggregate> Build(TSpec spec, CancellationToken token);
    }
}