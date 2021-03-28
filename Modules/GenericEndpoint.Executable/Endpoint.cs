namespace SpaceEngineers.Core.GenericEndpoint.Executable
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using GenericEndpoint.Abstractions;
    using Internals;

    /// <summary>
    /// Executable endpoint entry point
    /// </summary>
    public static class Endpoint
    {
        /// <summary>
        /// Starts generic endpoint asynchronously
        /// </summary>
        /// <param name="endpointOptions">Endpoint options</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing start operation</returns>
        public static async Task<IGenericEndpoint> StartAsync(
            EndpointOptions endpointOptions,
            CancellationToken token)
        {
            var dependencyContainer = DependencyContainerPerEndpoint(endpointOptions);

            var endpoint = dependencyContainer.Resolve<IRunnableEndpoint>();
            await endpoint.StartAsync(token).ConfigureAwait(false);

            return dependencyContainer.Resolve<GenericEndpoint>();
        }

        private static IDependencyContainer DependencyContainerPerEndpoint(EndpointOptions endpointOptions)
        {
            var containerOptions = endpointOptions.ContainerOptions ?? new DependencyContainerOptions();

            var registrations = new List<IManualRegistration>(containerOptions.ManualRegistrations)
            {
                new GenericEndpointIdentityManualRegistration(endpointOptions.Identity)
            };

            containerOptions.ManualRegistrations = registrations;

            return DependencyContainer.CreateBoundedAbove(containerOptions, endpointOptions.AboveAssemblies.ToArray());
        }
    }
}