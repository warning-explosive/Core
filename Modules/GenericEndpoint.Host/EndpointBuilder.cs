namespace SpaceEngineers.Core.GenericEndpoint.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration;
    using Basics;
    using Contract;

    /// <summary>
    /// EndpointBuilder
    /// </summary>
    public class EndpointBuilder
    {
        private readonly Assembly[] _rootAssemblies;

        internal EndpointBuilder()
        {
            _rootAssemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint)))
            };

            EndpointPluginAssemblies = Array.Empty<Assembly>();
            Modifiers = Array.Empty<Func<DependencyContainerOptions, DependencyContainerOptions>>();
        }

        /// <summary>
        /// Endpoint plugin assemblies
        /// </summary>
        public IReadOnlyCollection<Assembly> EndpointPluginAssemblies { get; private set; }

        private IReadOnlyCollection<Func<DependencyContainerOptions, DependencyContainerOptions>> Modifiers { get; set; }

        /// <summary>
        /// With endpoint plugin assemblies
        /// </summary>
        /// <param name="assemblies">Endpoint plugin assemblies</param>
        /// <returns>EndpointBuilder</returns>
        public EndpointBuilder WithEndpointPluginAssemblies(params Assembly[] assemblies)
        {
            return new EndpointBuilder
            {
                EndpointPluginAssemblies = EndpointPluginAssemblies.Concat(assemblies).ToList(),
                Modifiers = Modifiers
            };
        }

        /// <summary>
        /// With default cross cutting concerns
        /// </summary>
        /// <returns>EndpointBuilder</returns>
        public EndpointBuilder WithDefaultCrossCuttingConcerns()
        {
            var crossCuttingConcernsAssembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CrossCuttingConcerns)));

            return new EndpointBuilder
            {
                EndpointPluginAssemblies = EndpointPluginAssemblies.Concat(new[] { crossCuttingConcernsAssembly }).ToList(),
                Modifiers = Modifiers
            };
        }

        /// <summary>
        /// With statistics
        /// </summary>
        /// <returns>EndpointBuilder</returns>
        public EndpointBuilder WithStatistics()
        {
            var genericEndpointStatisticsAssembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.Statistics)));

            return new EndpointBuilder
            {
                EndpointPluginAssemblies = EndpointPluginAssemblies.Concat(new[] { genericEndpointStatisticsAssembly }).ToList(),
                Modifiers = Modifiers
            };
        }

        /// <summary>
        /// Modify container options
        /// </summary>
        /// <param name="modifier">Modifier</param>
        /// <returns>EndpointBuilder</returns>
        public EndpointBuilder ModifyContainerOptions(Func<DependencyContainerOptions, DependencyContainerOptions> modifier)
        {
            return new EndpointBuilder
            {
                EndpointPluginAssemblies = EndpointPluginAssemblies,
                Modifiers = Modifiers.Concat(new[] { modifier }).ToList()
            };
        }

        /// <summary>
        /// Build endpoint options
        /// </summary>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <returns>EndpointOptions</returns>
        public EndpointOptions BuildOptions(EndpointIdentity endpointIdentity)
        {
            var containerOptions = new DependencyContainerOptions();

            if (Modifiers.Any())
            {
                containerOptions = Modifiers.Aggregate(containerOptions, (current, modifier) => modifier(current));
            }

            return new EndpointOptions(
                endpointIdentity,
                containerOptions,
                _rootAssemblies.Concat(EndpointPluginAssemblies).ToArray());
        }
    }
}