namespace SpaceEngineers.Core.DataAccess.Orm.Host.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CompositionRoot;
    using Connection;
    using Registrations;

    internal class MigrationsEndpointBuilder : IMigrationsEndpointBuilder
    {
        public MigrationsEndpointBuilder()
        {
            EndpointPluginAssemblies = Array.Empty<Assembly>();
            Modifiers = Array.Empty<Func<DependencyContainerOptions, DependencyContainerOptions>>();
        }

        public IReadOnlyCollection<Assembly> EndpointPluginAssemblies { get; private set; }

        public IReadOnlyCollection<Func<DependencyContainerOptions, DependencyContainerOptions>> Modifiers { get; private set; }

        public IMigrationsEndpointBuilder WithDataAccess(IDatabaseProvider databaseProvider)
        {
            var modifier = new Func<DependencyContainerOptions, DependencyContainerOptions>(options => options.WithManualRegistrations(new DatabaseProviderManualRegistration(databaseProvider)));

            Modifiers = Modifiers
               .Concat(new[] { modifier })
               .ToList();

            EndpointPluginAssemblies = EndpointPluginAssemblies
               .Concat(databaseProvider.Migrations())
               .ToList();

            return this;
        }

        public IMigrationsEndpointBuilder WithEndpointPluginAssemblies(params Assembly[] assemblies)
        {
            EndpointPluginAssemblies = EndpointPluginAssemblies
               .Concat(assemblies)
               .ToList();

            return this;
        }

        public IMigrationsEndpointBuilder ModifyContainerOptions(Func<DependencyContainerOptions, DependencyContainerOptions> modifier)
        {
            Modifiers = Modifiers
               .Concat(new[] { modifier })
               .ToList();

            return this;
        }

        public MigrationsEndpointOptions BuildOptions()
        {
            var containerOptions = new DependencyContainerOptions();

            if (Modifiers.Any())
            {
                containerOptions = Modifiers.Aggregate(containerOptions, (current, modifier) => modifier(current));
            }

            return new MigrationsEndpointOptions(
                containerOptions,
                EndpointPluginAssemblies.ToArray());
        }
    }
}