namespace SpaceEngineers.Core.GenericEndpoint.Host.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using Basics;
    using CompositionRoot;
    using Contract;
    using Core.DataAccess.Orm;
    using Overrides;

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

        private IReadOnlyCollection<Assembly> EndpointPluginAssemblies { get; init; }

        private IReadOnlyCollection<Func<DependencyContainerOptions, DependencyContainerOptions>> Modifiers { get; init; }

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
            var genericEndpointDataAccessAssembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.DataAccess)));
            var dataAccessModifier = new Func<DependencyContainerOptions, DependencyContainerOptions>(options => options.WithOverrides(new DataAccessOverrides()));

            return new EndpointBuilder
            {
                Modifiers = Modifiers
                    .Concat(new[] { dataAccessModifier })
                    .ToList(),
                EndpointPluginAssemblies = EndpointPluginAssemblies
                    .Concat(new[] { genericEndpointDataAccessAssembly })
                    .Concat(databaseProvider.Implementation())
                    .ToList()
            };
        }

        public IEndpointBuilder WithTracing()
        {
            var genericEndpointTracingAssembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.Tracing)));

            return new EndpointBuilder
            {
                EndpointPluginAssemblies = EndpointPluginAssemblies.Concat(new[] { genericEndpointTracingAssembly }).ToList(),
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