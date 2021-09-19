namespace SpaceEngineers.Core.GenericEndpoint.Host.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using Contract;
    using Core.DataAccess.Orm;
    using DataAccess;
    using GenericHost.Api.Abstractions;
    using Overrides;

    internal class EndpointBuilder : IEndpointBuilder
    {
        private readonly Assembly[] _rootAssemblies;

        internal EndpointBuilder()
            : this(
                new[] { AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint))) },
                Array.Empty<Assembly>(),
                Array.Empty<Func<DependencyContainerOptions, DependencyContainerOptions>>(),
                Array.Empty<Func<IDependencyContainer, IHostStartupAction>>(),
                Array.Empty<Func<IDependencyContainer, IHostBackgroundWorker>>())
        {
        }

        private EndpointBuilder(
            Assembly[] rootAssemblies,
            IReadOnlyCollection<Assembly> endpointPluginAssemblies,
            IReadOnlyCollection<Func<DependencyContainerOptions, DependencyContainerOptions>> modifiers,
            IReadOnlyCollection<Func<IDependencyContainer, IHostStartupAction>> startupActions,
            IReadOnlyCollection<Func<IDependencyContainer, IHostBackgroundWorker>> backgroundWorkers)
        {
            _rootAssemblies = rootAssemblies;
            EndpointPluginAssemblies = endpointPluginAssemblies;
            Modifiers = modifiers;
            StartupActions = startupActions;
            BackgroundWorkers = backgroundWorkers;
        }

        public IReadOnlyCollection<Func<IDependencyContainer, IHostStartupAction>> StartupActions { get; }

        public IReadOnlyCollection<Func<IDependencyContainer, IHostBackgroundWorker>> BackgroundWorkers { get; }

        private IReadOnlyCollection<Assembly> EndpointPluginAssemblies { get; }

        private IReadOnlyCollection<Func<DependencyContainerOptions, DependencyContainerOptions>> Modifiers { get; }

        public IEndpointBuilder WithEndpointPluginAssemblies(params Assembly[] assemblies)
        {
            return new EndpointBuilder(
                _rootAssemblies,
                EndpointPluginAssemblies.Concat(assemblies).ToList(),
                Modifiers,
                StartupActions,
                BackgroundWorkers);
        }

        public IEndpointBuilder WithDefaultCrossCuttingConcerns()
        {
            var crossCuttingConcernsAssembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CrossCuttingConcerns)));

            return new EndpointBuilder(
                _rootAssemblies,
                EndpointPluginAssemblies.Concat(new[] { crossCuttingConcernsAssembly }).ToList(),
                Modifiers,
                StartupActions,
                BackgroundWorkers);
        }

        public IEndpointBuilder WithTracing()
        {
            var genericEndpointTracingAssembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.Tracing)));

            return new EndpointBuilder(
                _rootAssemblies,
                EndpointPluginAssemblies.Concat(new[] { genericEndpointTracingAssembly }).ToList(),
                Modifiers,
                StartupActions,
                BackgroundWorkers);
        }

        public IEndpointBuilder WithDataAccess(IDatabaseProvider databaseProvider)
        {
            var genericEndpointDataAccessAssembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.DataAccess)));
            var dataAccessModifier = new Func<DependencyContainerOptions, DependencyContainerOptions>(options => options.WithOverrides(new DataAccessOverrides()));
            var backgroundWorkerProducer = new Func<IDependencyContainer, IHostBackgroundWorker>(dependencyContainer => new GenericEndpointOutboxHostBackgroundWorker(dependencyContainer));

            return new EndpointBuilder(
                _rootAssemblies,
                EndpointPluginAssemblies.Concat(new[] { genericEndpointDataAccessAssembly }).Concat(databaseProvider.Implementation()).ToList(),
                Modifiers.Concat(new[] { dataAccessModifier }).ToList(),
                StartupActions,
                BackgroundWorkers.Concat(new[] { backgroundWorkerProducer }).ToList());
        }

        public IEndpointBuilder ModifyContainerOptions(Func<DependencyContainerOptions, DependencyContainerOptions> modifier)
        {
            return new EndpointBuilder(
                _rootAssemblies,
                EndpointPluginAssemblies,
                Modifiers.Concat(new[] { modifier }).ToList(),
                StartupActions,
                BackgroundWorkers);
        }

        public IEndpointBuilder WithStartupAction(Func<IDependencyContainer, IHostStartupAction> producer)
        {
            return new EndpointBuilder(
                _rootAssemblies,
                EndpointPluginAssemblies,
                Modifiers,
                StartupActions.Concat(new[] { producer }).ToList(),
                BackgroundWorkers);
        }

        public IEndpointBuilder WithBackgroundWorker(Func<IDependencyContainer, IHostBackgroundWorker> producer)
        {
            return new EndpointBuilder(
                _rootAssemblies,
                EndpointPluginAssemblies,
                Modifiers,
                StartupActions,
                BackgroundWorkers.Concat(new[] { producer }).ToList());
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