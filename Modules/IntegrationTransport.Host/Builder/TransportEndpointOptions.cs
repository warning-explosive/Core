namespace SpaceEngineers.Core.IntegrationTransport.Host.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CompositionRoot;
    using GenericEndpoint.Contract;

    /// <summary>
    /// Transport endpoint initialization options
    /// </summary>
    public class TransportEndpointOptions
    {
        /// <summary> .cctor </summary>
        /// <param name="endpointIdentity">Endpoint identity</param>
        /// <param name="containerOptions">DependencyContainerOptions</param>
        /// <param name="aboveAssemblies">Assemblies that limits assembly loading for endpoint's dependency container</param>
        internal TransportEndpointOptions(
            EndpointIdentity endpointIdentity,
            DependencyContainerOptions containerOptions,
            params Assembly[] aboveAssemblies)
        {
            EndpointIdentity = endpointIdentity;
            ContainerOptions = containerOptions;

            if (!aboveAssemblies.Any())
            {
                throw new InvalidOperationException($"Transport endpoint {endpointIdentity} should be limited at least by one above assembly");
            }

            AboveAssemblies = aboveAssemblies;
        }

        /// <summary>
        /// Endpoint identity
        /// </summary>
        public EndpointIdentity EndpointIdentity { get; }

        /// <summary>
        /// Dependency container options
        /// </summary>
        public DependencyContainerOptions ContainerOptions { get; }

        /// <summary>
        /// Assemblies that limits assembly loading for endpoint's dependency container
        /// </summary>
        public IReadOnlyCollection<Assembly> AboveAssemblies { get; }
    }
}