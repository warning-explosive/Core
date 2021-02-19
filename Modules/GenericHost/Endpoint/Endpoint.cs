namespace SpaceEngineers.Core.GenericHost.Endpoint
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;

    /// <summary>
    /// Generic endpoint entry point
    /// </summary>
    public static class Endpoint
    {
        /// <summary>
        /// Create and start several generic endpoint instances
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <param name="endpointOptionsCollection">Endpoints options collection</param>
        /// <returns>IGenericEndpoints collection</returns>
        public static async Task<ICompositeEndpoint> StartAsync(
            CancellationToken token,
            params EndpointOptions[] endpointOptionsCollection)
        {
            var endpoints = endpointOptionsCollection
                .Select(endpointOptions => (IGenericEndpoint)new GenericEndpoint(endpointOptions))
                .ToArray();

            var compositeEndpoint = new CompositeEndpoint(endpoints);

            await compositeEndpoint.StopAsync().ConfigureAwait(false);

            return compositeEndpoint;
        }

        /// <summary>
        /// Create and start generic endpoint instance
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <param name="endpointOptions">Endpoint options</param>
        /// <returns>IGenericEndpoint</returns>
        public static async Task<IGenericEndpoint> StartAsync(
            CancellationToken token,
            EndpointOptions endpointOptions)
        {
            var endpoint = new GenericEndpoint(endpointOptions);

            await endpoint.StartAsync(token).ConfigureAwait(false);

            return endpoint;
        }
    }
}