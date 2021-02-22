namespace SpaceEngineers.Core.GenericHost
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Enumerations;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using Internals;

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
            var startingEndpoints = endpointOptionsCollection
                .Select(endpointOptions => StartAsync(token, endpointOptions))
                .ToArray();

            var endpoints = await Task.WhenAll(startingEndpoints).ConfigureAwait(false);

            return new CompositeEndpoint(endpoints);
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
                new GenericEndpointManualRegistration(endpointOptions.Identity)
            };

            containerOptions.ManualRegistrations = registrations;

            return endpointOptions.Assembly != null
                ? DependencyContainer.CreateBoundedAbove(endpointOptions.Assembly, containerOptions)
                : DependencyContainer.Create(containerOptions);
        }

        private class GenericEndpointManualRegistration : IManualRegistration
        {
            private readonly EndpointIdentity _endpointIdentity;

            public GenericEndpointManualRegistration(EndpointIdentity endpointIdentity)
            {
                _endpointIdentity = endpointIdentity;
            }

            public void Register(IRegistrationContainer container)
            {
                container.RegisterInstance(_endpointIdentity);

                container.Register<IGenericEndpoint, GenericEndpoint>(EnLifestyle.Singleton);
                container.Register<IRunnableEndpoint, GenericEndpoint>(EnLifestyle.Singleton);
                container.Register<IExecutableEndpoint, GenericEndpoint>(EnLifestyle.Singleton);
                container.Register<IMessagePipeline, GenericEndpoint>(EnLifestyle.Singleton);
                container.Register<GenericEndpoint, GenericEndpoint>(EnLifestyle.Singleton);
            }
        }
    }
}