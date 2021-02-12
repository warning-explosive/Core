namespace SpaceEngineers.Core.GenericHost
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Enumerations;
    using Internals;

    /// <summary>
    /// Generic endpoint entry point
    /// </summary>
    public static class Endpoint
    {
        /// <summary>
        /// Create and start several generic endpoint instances
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="endpointOptionsCollection">Endpoints options collection</param>
        /// <returns>IGenericEndpoints collection</returns>
        public static async Task<ICompositeEndpoint> StartAsync(
            CancellationToken cancellationToken,
            params EndpointOptions[] endpointOptionsCollection)
        {
            IGenericEndpoint[] endpoints = endpointOptionsCollection
                .Select(endpointOptions => new GenericEndpoint(endpointOptions.Identity, DependencyContainerPerEndpoint(endpointOptions)))
                .ToArray();

            var compositeEndpoint = new CompositeEndpoint(endpoints);

            await compositeEndpoint.StopAsync().ConfigureAwait(false);

            return compositeEndpoint;
        }

        /// <summary>
        /// Create and start generic endpoint instance
        /// </summary>
        /// <param name="endpointOptions">Endpoint options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>IGenericEndpoint</returns>
        public static async Task<IGenericEndpoint> StartAsync(
            EndpointOptions endpointOptions,
            CancellationToken cancellationToken)
        {
            var endpoint = new GenericEndpoint(endpointOptions.Identity, DependencyContainerPerEndpoint(endpointOptions));

            await endpoint.StartAsync(cancellationToken).ConfigureAwait(false);

            return endpoint;
        }

        private static IDependencyContainer DependencyContainerPerEndpoint(EndpointOptions endpointOptions)
        {
            var containerOptions = endpointOptions.ContainerOptions ?? new DependencyContainerOptions();

            containerOptions.OnRegistration += (_, e) =>
            {
                e.Registration.Register<EndpointIdentity>(() => endpointOptions.Identity, EnLifestyle.Singleton);
            };

            return endpointOptions.Assembly != null
                ? DependencyContainer.CreateBoundedAbove(endpointOptions.Assembly, containerOptions)
                : DependencyContainer.Create(containerOptions);
        }
    }
}