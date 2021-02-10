namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiringApi.Abstractions;

    /// <summary>
    /// Endpoint initializer abstraction
    /// </summary>
    public interface IEndpointInitializer : ICollectionResolvable<IEndpointInitializer>
    {
        /// <summary>
        /// Initialize endpoint
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Ongoing initialization operation</returns>
        Task Initialize(CancellationToken cancellationToken);
    }
}