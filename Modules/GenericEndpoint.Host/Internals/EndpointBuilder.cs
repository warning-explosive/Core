namespace SpaceEngineers.Core.GenericEndpoint.Host.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using Basics;
    using CompositionRoot;
    using Contract;

    internal class EndpointBuilder : IEndpointBuilder
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

        private IReadOnlyCollection<Assembly> EndpointPluginAssemblies { get; set; }

        private IReadOnlyCollection<Func<DependencyContainerOptions, DependencyContainerOptions>> Modifiers { get; set; }

        public IEndpointBuilder WithEndpointPluginAssemblies(params Assembly[] assemblies)
        {
            return new EndpointBuilder
            {
                EndpointPluginAssemblies = EndpointPluginAssemblies.Concat(assemblies).ToList(),
                Modifiers = Modifiers
            };
        }

        public IEndpointBuilder WithDefaultCrossCuttingConcerns()
        {
            var crossCuttingConcernsAssembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CrossCuttingConcerns)));

            return new EndpointBuilder
            {
                EndpointPluginAssemblies = EndpointPluginAssemblies.Concat(new[] { crossCuttingConcernsAssembly }).ToList(),
                Modifiers = Modifiers
            };
        }

        public IEndpointBuilder WithDataAccess(IDatabaseProvider databaseProvider)
        {
            var genericEndpointDataAccessAssembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(nameof(SpaceEngineers),
                nameof(Core),
                nameof(Core.GenericEndpoint),
                "DataAccess"));

            return new EndpointBuilder
            {
                EndpointPluginAssemblies = EndpointPluginAssemblies
                    .Concat(new[] { genericEndpointDataAccessAssembly })
                    .Concat(databaseProvider.Implementation())
                    .ToList(),
                Modifiers = Modifiers
            };
        }

        public IEndpointBuilder WithStatistics()
        {
            var genericEndpointStatisticsAssembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.Statistics)));

            return new EndpointBuilder
            {
                EndpointPluginAssemblies = EndpointPluginAssemblies.Concat(new[] { genericEndpointStatisticsAssembly }).ToList(),
                Modifiers = Modifiers
            };
        }

        public IEndpointBuilder ModifyContainerOptions(Func<DependencyContainerOptions, DependencyContainerOptions> modifier)
        {
            return new EndpointBuilder
            {
                EndpointPluginAssemblies = EndpointPluginAssemblies,
                Modifiers = Modifiers.Concat(new[] { modifier }).ToList()
            };
        }

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