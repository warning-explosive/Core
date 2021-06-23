namespace SpaceEngineers.Core.GenericEndpoint
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration;
    using Contract;

    /// <summary>
    /// Endpoint initialization options
    /// </summary>
    public class EndpointOptions
    {
        /// <summary> .cctor </summary>
        /// <param name="identity">EndpointIdentity</param>
        /// <param name="containerOptions">DependencyContainerOptions</param>
        /// <param name="aboveAssemblies">Assemblies that limits assembly loading for endpoint's dependency container</param>
        public EndpointOptions(EndpointIdentity identity, DependencyContainerOptions containerOptions, params Assembly[] aboveAssemblies)
        {
            Identity = identity;
            ContainerOptions = containerOptions;

            if (!aboveAssemblies.Any())
            {
                throw new InvalidOperationException($"Endpoint {identity} should be limited at least by one above assembly");
            }

            AboveAssemblies = aboveAssemblies;
        }

        /// <summary>
        /// Endpoint identity
        /// </summary>
        public EndpointIdentity Identity { get; }

        /// <summary>
        /// Dependency container options
        /// </summary>
        public DependencyContainerOptions ContainerOptions { get; }

        /// <summary>
        /// Assemblies that limits assembly loading for endpoint's dependency container
        /// </summary>
        public IReadOnlyCollection<Assembly> AboveAssemblies { get; }

        /// <summary>
        /// With container options
        /// </summary>
        /// <param name="containerOptions">DependencyContainerOptions</param>
        /// <returns>EndpointOptions</returns>
        public EndpointOptions WithContainerOptions(DependencyContainerOptions containerOptions)
        {
            return new EndpointOptions(Identity, containerOptions, AboveAssemblies.ToArray());
        }

        /// <summary>
        /// With above assemblies
        /// </summary>
        /// <param name="assemblies">Assemblies</param>
        /// <returns>EndpointOptions</returns>
        public EndpointOptions WithAboveAssemblies(params Assembly[] assemblies)
        {
            return new EndpointOptions(Identity, ContainerOptions, AboveAssemblies.Concat(assemblies).ToArray());
        }
    }
}