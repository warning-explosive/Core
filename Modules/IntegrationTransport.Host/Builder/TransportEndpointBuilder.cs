namespace SpaceEngineers.Core.IntegrationTransport.Host.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Api.Abstractions;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using GenericHost.Api.Abstractions;
    using InMemory;
    using InMemory.ManualRegistrations;
    using Microsoft.Extensions.Hosting;

    internal class TransportEndpointBuilder : ITransportEndpointBuilder
    {
        private const string RequireWithContainerCall = ".WithContainer() should be called during endpoint declaration";

        private readonly Assembly[] _rootAssemblies;
        private readonly Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>>? _containerImplementationProducer;

        internal TransportEndpointBuilder()
            : this(
                new[] { AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.IntegrationTransport))) },
                null,
                Array.Empty<Assembly>(),
                Array.Empty<Func<DependencyContainerOptions, DependencyContainerOptions>>(),
                Array.Empty<Func<IDependencyContainer, IHostStartupAction>>(),
                Array.Empty<Func<IDependencyContainer, IHostBackgroundWorker>>())
        {
        }

        private TransportEndpointBuilder(
            Assembly[] rootAssemblies,
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>>? containerImplementationProducer,
            IReadOnlyCollection<Assembly> endpointPluginAssemblies,
            IReadOnlyCollection<Func<DependencyContainerOptions, DependencyContainerOptions>> modifiers,
            IReadOnlyCollection<Func<IDependencyContainer, IHostStartupAction>> startupActions,
            IReadOnlyCollection<Func<IDependencyContainer, IHostBackgroundWorker>> backgroundWorkers)
        {
            _rootAssemblies = rootAssemblies;
            _containerImplementationProducer = containerImplementationProducer;
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
                _containerImplementationProducer,
                EndpointPluginAssemblies.Concat(assemblies).ToList(),
                Modifiers,
                StartupActions,
                BackgroundWorkers);
        }

        public ITransportEndpointBuilder WithContainer(
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> containerImplementationProducer)
        {
            return new TransportEndpointBuilder(
                _rootAssemblies,
                containerImplementationProducer,
                EndpointPluginAssemblies,
                Modifiers,
                StartupActions,
                BackgroundWorkers);
        }

        public ITransportEndpointBuilder ModifyContainerOptions(Func<DependencyContainerOptions, DependencyContainerOptions> modifier)
        {
            return new TransportEndpointBuilder(
                _rootAssemblies,
                _containerImplementationProducer,
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
                _containerImplementationProducer,
                EndpointPluginAssemblies.Concat(new[] { crossCuttingConcernsAssembly }).ToList(),
                Modifiers,
                StartupActions,
                BackgroundWorkers);
        }

        public ITransportEndpointBuilder WithTracing()
        {
            var integrationTransportTracingAssembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.IntegrationTransport), nameof(Core.IntegrationTransport.Tracing)));

            return new TransportEndpointBuilder(
                _rootAssemblies,
                _containerImplementationProducer,
                EndpointPluginAssemblies.Concat(new[] { integrationTransportTracingAssembly }).ToList(),
                Modifiers,
                StartupActions,
                BackgroundWorkers);
        }

        public ITransportEndpointBuilder WithInMemoryIntegrationTransport(IHostBuilder hostBuilder)
        {
            var inMemoryIntegrationTransportAssembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.IntegrationTransport), nameof(Core.IntegrationTransport.InMemory)));

            var endpointInstanceSelectionBehavior = new EndpointInstanceSelectionBehavior();
            var inMemoryIntegrationTransport = new InMemoryIntegrationTransport(endpointInstanceSelectionBehavior);
            var inMemoryIntegrationTransportManualRegistration = new InMemoryIntegrationTransportManualRegistration(inMemoryIntegrationTransport, endpointInstanceSelectionBehavior);

            hostBuilder.Properties[nameof(IIntegrationTransport)] = inMemoryIntegrationTransportManualRegistration;

            var inMemoryIntegrationTransportModifier = new Func<DependencyContainerOptions, DependencyContainerOptions>(options => options.WithManualRegistrations(inMemoryIntegrationTransportManualRegistration));

            return new TransportEndpointBuilder(
                    _rootAssemblies,
                    _containerImplementationProducer,
                    EndpointPluginAssemblies.Concat(new[] { inMemoryIntegrationTransportAssembly }).ToList(),
                    Modifiers.Concat(new[] { inMemoryIntegrationTransportModifier }).ToList(),
                    StartupActions,
                    BackgroundWorkers);
        }

        public ITransportEndpointBuilder WithStartupAction(Func<IDependencyContainer, IHostStartupAction> producer)
        {
            return new TransportEndpointBuilder(
                _rootAssemblies,
                _containerImplementationProducer,
                EndpointPluginAssemblies,
                Modifiers,
                StartupActions.Concat(new[] { producer }).ToList(),
                BackgroundWorkers);
        }

        public ITransportEndpointBuilder WithBackgroundWorker(Func<IDependencyContainer, IHostBackgroundWorker> producer)
        {
            return new TransportEndpointBuilder(
                _rootAssemblies,
                _containerImplementationProducer,
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
                _containerImplementationProducer ?? throw new InvalidOperationException(RequireWithContainerCall),
                _rootAssemblies.Concat(EndpointPluginAssemblies).ToArray());
        }
    }
}