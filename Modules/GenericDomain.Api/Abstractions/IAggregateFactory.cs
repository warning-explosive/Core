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
        where TAggregate : class, IAggregate<TAggregate>
        where TSpec : IAggregateSpecification
    {
        /// <summary>
        /// Builds aggregate
        /// </summary>
        /// <param name="spec">Specification</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Built aggregate</returns>
        Task<TAggregate> Build(TSpec spec, CancellationToken token);
    }
}