namespace SpaceEngineers.Core.IntegrationTransport.Host.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using GenericHost.Api.Abstractions;
    using InMemory.ManualRegistrations;

    internal class TransportEndpointBuilder : ITransportEndpointBuilder
    {
        private readonly Assembly[] _rootAssemblies;

        internal TransportEndpointBuilder()
            : this(
                new[] { AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.IntegrationTransport))) },
                Array.Empty<Assembly>(),
                Array.Empty<Func<DependencyContainerOptions, DependencyContainerOptions>>(),
                Array.Empty<Func<IDependencyContainer, IHostStartupAction>>(),
                Array.Empty<Func<IDependencyContainer, IHostBackgroundWorker>>())
        {
        }

        private TransportEndpointBuilder(
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

        public ITransportEndpointBuilder WithEndpointPluginAssemblies(params Assembly[] assemblies)
        {
            return new TransportEndpointBuilder(
                _rootAssemblies,
                EndpointPluginAssemblies.Concat(assemblies).ToList(),
                Modifiers,
                StartupActions,
                BackgroundWorkers);
        }

        public ITransportEndpointBuilder ModifyContainerOptions(Func<DependencyContainerOptions, DependencyContainerOptions> modifier)
        {
            return new TransportEndpointBuilder(
                _rootAssemblies,
                EndpointPluginAssemblies,
                Modifiers.Concat(new[] { modifier }).ToList(),
                StartupActions,
                BackgroundWorkers);
        }

        public ITransportEndpointBuilder WithDefaultCrossCuttingConcerns()
        {
            var crossCuttingConcernsAssembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CrossCuttingConcerns)));

            return new TransportEndpointBuilder(
                _rootAssemblies,
                EndpointPluginAssemblies.Concat(new[] { crossCuttingConcernsAssembly }).ToList(),
                Modifiers,
                StartupActions,
                BackgroundWorkers);
        }

        public ITransportEndpointBuilder WithInMemoryIntegrationTransport()
        {
            var inMemoryIntegrationTransportAssembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.IntegrationTransport), nameof(Core.IntegrationTransport.InMemory)));
            var inMemoryIntegrationTransportModifier = new Func<DependencyContainerOptions, DependencyContainerOptions>(options => options.WithManualRegistrations(new InMemoryIntegrationTransportManualRegistration()));

            return new TransportEndpointBuilder(
                    _rootAssemblies,
                    EndpointPluginAssemblies.Concat(new[] { inMemoryIntegrationTransportAssembly }).ToList(),
                    Modifiers.Concat(new[] { inMemoryIntegrationTransportModifier }).ToList(),
                    StartupActions,
                    BackgroundWorkers);
        }

        public ITransportEndpointBuilder WithStartupAction(Func<IDependencyContainer, IHostStartupAction> producer)
        {
            return new TransportEndpointBuilder(
                _rootAssemblies,
                EndpointPluginAssemblies,
                Modifiers,
                StartupActions.Concat(new[] { producer }).ToList(),
                BackgroundWorkers);
        }

        public ITransportEndpointBuilder WithBackgroundWorker(Func<IDependencyContainer, IHostBackgroundWorker> producer)
        {
            return new TransportEndpointBuilder(
                _rootAssemblies,
                EndpointPluginAssemblies,
                Modifiers,
                StartupActions,
                BackgroundWorkers.Concat(new[] { producer }).ToList());
        }

        public TransportEndpointOptions BuildOptions()
        {
            var containerOptions = new DependencyContainerOptions();

            if (Modifiers.Any())
            {
                containerOptions = Modifiers.Aggregate(containerOptions, (current, modifier) => modifier(current));
            }

            return new TransportEndpointOptions(
                containerOptions,
                _rootAssemblies.Concat(EndpointPluginAssemblies).ToArray());
        }
    }
}