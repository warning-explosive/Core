namespace SpaceEngineers.Core.DataAccess.Orm.Host.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CompositionRoot;

    /// <summary>
    /// MigrationsEndpointOptions
    /// </summary>
    public class MigrationsEndpointOptions
    {
        /// <summary> .cctor </summary>
        /// <param name="containerOptions">DependencyContainerOptions</param>
        /// <param name="aboveAssemblies">Assemblies that limits assembly loading for endpoint's dependency container</param>
        internal MigrationsEndpointOptions(
            DependencyContainerOptions containerOptions,
            params Assembly[] aboveAssemblies)
        {
            ContainerOptions = containerOptions;

            if (!aboveAssemblies.Any())
            {
                throw new InvalidOperationException("Migrations endpoint should be limited at least by one above assembly");
            }

            AboveAssemblies = aboveAssemblies;
        }

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