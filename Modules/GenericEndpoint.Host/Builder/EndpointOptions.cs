namespace SpaceEngineers.Core.GenericEndpoint.Host.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using Contract;

    /// <summary>
    /// Endpoint initialization options
    /// </summary>
    public class EndpointOptions
    {
        /// <summary> .cctor </summary>
        /// <param name="identity">EndpointIdentity</param>
        /// <param name="containerOptions">DependencyContainerOptions</param>
        /// <param name="containerImplementationProducer">Container implementation producer</param>
        /// <param name="aboveAssemblies">Assemblies that limits assembly loading for endpoint's dependency container</param>
        internal EndpointOptions(
            EndpointIdentity identity,
            DependencyContainerOptions containerOptions,
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> containerImplementationProducer,
            params Assembly[] aboveAssemblies)
        {
            Identity = identity;
            ContainerOptions = containerOptions;
            ContainerImplementationProducer = containerImplementationProducer;

            if (!aboveAssemblies.Any())
            {
                throw new InvalidOperationException($"Endpoint {identity} should be limited at least by one above assembly");
            }

            AboveAssemblies = aboveAssemblies;
        }

        /// <summary>
        /// EndpointIdentity
        /// </summary>
        public EndpointIdentity Identity { get; }

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

        /// <summary>
        /// With container options
        /// </summary>
        /// <param name="containerOptions">DependencyContainerOptions</param>
        /// <returns>EndpointOptions</returns>
        internal EndpointOptions WithContainerOptions(DependencyContainerOptions containerOptions)
        {
            return new EndpointOptions(
                Identity,
                containerOptions,
                ContainerImplementationProducer,
                AboveAssemblies.ToArray());
        }
    }
}