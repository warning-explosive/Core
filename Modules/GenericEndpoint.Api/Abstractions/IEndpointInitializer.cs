namespace SpaceEngineers.Core.GenericEndpoint.Api.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// Endpoint initializer abstraction
    /// </summary>
    public interface IEndpointInitializer : ICollectionResolvable<IEndpointInitializer>
    {
        /// <summary>
        /// Initialize endpoint
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing initialization operation</returns>
        Task Initialize(CancellationToken token);
    }
}