namespace SpaceEngineers.Core.GenericHost
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Enumerations;
    using GenericEndpoint.Abstractions;
    using Implementations;

    /// <summary>
    /// Generic endpoint entry point
    /// </summary>
    public static class Endpoint
    {
        /// <summary>
        /// Create and start generic endpoint instance
        /// </summary>
        /// <param name="options">Endpoint options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>IGenericEndpoint</returns>
        public static async Task<IGenericEndpoint> StartAsync(EndpointOptions options, CancellationToken? cancellationToken = null)
        {
            var endpoint = new GenericEndpoint(options.Identity, DependencyContainerPerEndpoint(options));

            await endpoint.StartAsync(cancellationToken ?? CancellationToken.None).ConfigureAwait(false);

            return endpoint;
        }

        private static IDependencyContainer DependencyContainerPerEndpoint(EndpointOptions endpointOptions)
        {
            var containerOptions = new DependencyContainerOptions();

            containerOptions.OnRegistration += (s, e) =>
            {
                e.Registration.Register(() => endpointOptions.Identity, EnLifestyle.Singleton);
                e.Registration.RegisterEmptyCollection<IEndpointInitializer>();
            };

            return DependencyContainer.Create(containerOptions); // TODO: bounded by assembly
        }
    }
}