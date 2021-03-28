namespace SpaceEngineers.Core.GenericEndpoint.Executable
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration;
    using GenericEndpoint;

    /// <summary>
    /// Endpoint initialization options
    /// </summary>
    public class EndpointOptions
    {
        /// <summary> .cctor </summary>
        /// <param name="identity">Endpoint identity</param>
        /// <param name="aboveAssemblies">Assemblies that limits assembly loading for endpoint's dependency container</param>
        public EndpointOptions(EndpointIdentity identity, params Assembly[] aboveAssemblies)
        {
            Identity = identity;

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
        /// Assemblies that limits assembly loading for endpoint's dependency container
        /// </summary>
        public IReadOnlyCollection<Assembly> AboveAssemblies { get; set; }

        /// <summary>
        /// Dependency container options
        /// </summary>
        public DependencyContainerOptions? ContainerOptions { get; set; }
    }
}