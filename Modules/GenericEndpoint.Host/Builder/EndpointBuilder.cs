namespace SpaceEngineers.Core.GenericEndpoint.Host.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using Contract;
    using Core.DataAccess.Orm.Connection;
    using DataAccess.BackgroundWorkers;
    using GenericHost.Api.Abstractions;
    using Overrides;
    using Registrations;

    internal class EndpointBuilder : IEndpointBuilder
    {
        private readonly Assembly[] _rootAssemblies = new[]
        {
            AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint)))
        };

        internal EndpointBuilder(EndpointIdentity endpointIdentity)
        {
            EndpointIdentity = endpointIdentity;
            EndpointPluginAssemblies = Array.Empty<Assembly>();
            Modifiers = Array.Empty<Func<DependencyContainerOptions, DependencyContainerOptions>>();
            StartupActions = Array.Empty<Func<IDependencyContainer, IHostStartupAction>>();
            BackgroundWorkers = Array.Empty<Func<IDependencyContainer, IHostBackgroundWorker>>();
        }

        public EndpointIdentity EndpointIdentity { get; }

        public IReadOnlyCollection<Func<IDependencyContainer, IHostStartupAction>> StartupActions { get; protected set; }

        public IReadOnlyCollection<Func<IDependencyContainer, IHostBackgroundWorker>> BackgroundWorkers { get; protected set; }

        public IReadOnlyCollection<Assembly> EndpointPluginAssemblies { get; protected set; }

        public IReadOnlyCollection<Func<DependencyContainerOptions, DependencyContainerOptions>> Modifiers { get; protected set; }

        public IEndpointBuilder WithEndpointPluginAssemblies(params Assembly[] assemblies)
        {
            EndpointPluginAssemblies = EndpointPluginAssemblies
               .Concat(assemblies)
               .ToList();

            return this;
        }

        public IEndpointBuilder WithTracing()
        {
            var assembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.Tracing)));

            EndpointPluginAssemblies = EndpointPluginAssemblies
               .Concat(new[] { assembly })
               .ToList();

            return this;
        }

        public IEndpointBuilder WithDataAccess(IDatabaseProvider databaseProvider)
        {
            var assembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.DataAccess)));
            var modifier = new Func<DependencyContainerOptions, DependencyContainerOptions>(options => options.WithManualRegistrations(new DatabaseProviderManualRegistration(databaseProvider)).WithOverrides(new DataAccessOverride()));
            var backgroundWorker = new Func<IDependencyContainer, IHostBackgroundWorker>(dependencyContainer => new GenericEndpointDataAccessHostBackgroundWorker(dependencyContainer));

            EndpointPluginAssemblies = EndpointPluginAssemblies
               .Concat(new[] { assembly })
               .Concat(databaseProvider.Implementation())
               .ToList();

            Modifiers = Modifiers
               .Concat(new[] { modifier })
               .ToList();

            BackgroundWorkers = BackgroundWorkers
               .Concat(new[] { backgroundWorker })
               .ToList();

            return this;
        }

        public IEndpointBuilder ModifyContainerOptions(Func<DependencyContainerOptions, DependencyContainerOptions> modifier)
        {
            Modifiers = Modifiers
               .Concat(new[] { modifier })
               .ToList();

            return this;
        }

        public IEndpointBuilder WithStartupAction(Func<IDependencyContainer, IHostStartupAction> producer)
        {
            StartupActions = StartupActions
               .Concat(new[] { producer })
               .ToList();

            return this;
        }

        public IEndpointBuilder WithBackgroundWorker(Func<IDependencyContainer, IHostBackgroundWorker> producer)
        {
            BackgroundWorkers = BackgroundWorkers
               .Concat(new[] { producer })
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