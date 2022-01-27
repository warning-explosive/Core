namespace SpaceEngineers.Core.IntegrationTransport.Host.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using GenericEndpoint.Contract;

    /// <summary>
    /// Transport endpoint initialization options
    /// </summary>
    public class TransportEndpointOptions
    {
        /// <summary> .cctor </summary>
        /// <param name="endpointIdentity">Endpoint identity</param>
        /// <param name="containerOptions">DependencyContainerOptions</param>
        /// <param name="containerImplementationProducer">Container implementation producer</param>
        /// <param name="aboveAssemblies">Assemblies that limits assembly loading for endpoint's dependency container</param>
        internal TransportEndpointOptions(
            EndpointIdentity endpointIdentity,
            DependencyContainerOptions containerOptions,
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> containerImplementationProducer,
            params Assembly[] aboveAssemblies)
        {
            EndpointIdentity = endpointIdentity;
            ContainerOptions = containerOptions;
            ContainerImplementationProducer = containerImplementationProducer;

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
        /// Container implementation producer
        /// </summary>
        public Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> ContainerImplementationProducer { get; }

        /// <summary>
        /// Assemblies that limits assembly loading for endpoint's dependency container
        /// </summary>
        public IReadOnlyCollection<Assembly> AboveAssemblies { get; }
    }
}