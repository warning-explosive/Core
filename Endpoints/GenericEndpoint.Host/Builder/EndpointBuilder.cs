namespace SpaceEngineers.Core.GenericEndpoint.Host.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Basics;
    using CompositionRoot;
    using Contract;
    using Core.DataAccess.Orm.Host.Abstractions;
    using Overrides;
    using Registrations;

    internal class EndpointBuilder : IEndpointBuilder
    {
        private readonly Assembly[] _rootAssemblies = new[]
        {
            AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericEndpoint)))
        };

        internal EndpointBuilder(EndpointIdentity endpointIdentity)
        {
            EndpointIdentity = endpointIdentity;
            EndpointPluginAssemblies = Array.Empty<Assembly>();
            Modifiers = Array.Empty<Func<DependencyContainerOptions, DependencyContainerOptions>>();
        }

        public EndpointIdentity EndpointIdentity { get; }

        public IReadOnlyCollection<Assembly> EndpointPluginAssemblies { get; protected set; }

        public IReadOnlyCollection<Func<DependencyContainerOptions, DependencyContainerOptions>> Modifiers { get; protected set; }

        public IEndpointBuilder WithEndpointPluginAssemblies(params Assembly[] assemblies)
        {
            EndpointPluginAssemblies = EndpointPluginAssemblies
               .Concat(assemblies)
               .ToList();

            return this;
        }

        public IEndpointBuilder WithDataAccess(
            IDatabaseProvider databaseProvider,
            Action<DataAccessOptions>? dataAccessOptions)
        {
            var assembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericEndpoint), nameof(DataAccess)));

            var modifier = new Func<DependencyContainerOptions, DependencyContainerOptions>(options => options
               .WithManualRegistrations(new GenericEndpointDataAccessHostBackgroundWorkerManualRegistration())
               .WithOverrides(new DataAccessOverride()));

            EndpointPluginAssemblies = EndpointPluginAssemblies
               .Concat(new[] { assembly })
               .Concat(databaseProvider.Implementation())
               .ToList();

            Modifiers = Modifiers
               .Concat(new[] { modifier })
               .ToList();

            dataAccessOptions?.Invoke(new DataAccessOptions(this));

            return this;
        }

        public IEndpointBuilder ModifyContainerOptions(Func<DependencyContainerOptions, DependencyContainerOptions> modifier)
        {
            Modifiers = Modifiers
               .Concat(new[] { modifier })
               .ToList();

            return this;
        }

        public EndpointOptions BuildOptions()
        {
            var containerOptions = new DependencyContainerOptions();

            if (Modifiers.Any())
            {
                containerOptions = Modifiers.Aggregate(containerOptions, (current, modifier) => modifier(current));
            }

            return new EndpointOptions(
                EndpointIdentity,
                containerOptions,
                _rootAssemblies.Concat(EndpointPluginAssemblies).ToArray());
        }
    }
}