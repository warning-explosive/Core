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
            endpointOptions.ContainerOptions ??= new DependencyContainerOptions();

            endpointOptions.ContainerOptions.ManualRegistrations
                = new List<IManualRegistration>(endpointOptions.ContainerOptions.ManualRegistrations)
                {
                    new GenericEndpointIdentityManualRegistration(endpointOptions.Identity)
                };

            return DependencyContainer.CreateBoundedAbove(endpointOptions.ContainerOptions, endpointOptions.AboveAssemblies.ToArray());
        }
    }
}