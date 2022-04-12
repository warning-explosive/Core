namespace SpaceEngineers.Core.IntegrationTransport.Host.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericHost.Api.Abstractions;
    using InMemory;
    using InMemory.ManualRegistrations;
    using Microsoft.Extensions.Hosting;

    internal class TransportEndpointBuilder : ITransportEndpointBuilder
    {
        private readonly Assembly[] _rootAssemblies;

        internal TransportEndpointBuilder(EndpointIdentity endpointIdentity)
        {
            EndpointIdentity = endpointIdentity;
            _rootAssemblies = new[] { AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.IntegrationTransport))) };
            EndpointPluginAssemblies = Array.Empty<Assembly>();
            Modifiers = Array.Empty<Func<DependencyContainerOptions, DependencyContainerOptions>>();
            StartupActions = Array.Empty<Func<IDependencyContainer, IHostStartupAction>>();
            BackgroundWorkers = Array.Empty<Func<IDependencyContainer, IHostBackgroundWorker>>();
        }

        public EndpointIdentity EndpointIdentity { get; }

        public IReadOnlyCollection<Func<IDependencyContainer, IHostStartupAction>> StartupActions { get; private set; }

        public IReadOnlyCollection<Func<IDependencyContainer, IHostBackgroundWorker>> BackgroundWorkers { get; private set; }

        public IReadOnlyCollection<Assembly> EndpointPluginAssemblies { get; private set; }

        public IReadOnlyCollection<Func<DependencyContainerOptions, DependencyContainerOptions>> Modifiers { get; private set; }

        public ITransportEndpointBuilder WithEndpointPluginAssemblies(params Assembly[] assemblies)
        {
            EndpointPluginAssemblies = EndpointPluginAssemblies
               .Concat(assemblies)
               .ToList();

            return this;
        }

        public ITransportEndpointBuilder ModifyContainerOptions(Func<DependencyContainerOptions, DependencyContainerOptions> modifier)
        {
            Modifiers = Modifiers
               .Concat(new[] { modifier })
               .ToList();

            return this;
        }

        public ITransportEndpointBuilder WithTracing()
        {
            var integrationTransportTracingAssembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(
                    nameof(SpaceEngineers),
                    nameof(Core),
                    nameof(Core.IntegrationTransport),
                    nameof(Core.IntegrationTransport.Tracing)));

            EndpointPluginAssemblies = EndpointPluginAssemblies
               .Concat(new[] { integrationTransportTracingAssembly })
               .ToList();

            return this;
        }

        public ITransportEndpointBuilder WithInMemoryIntegrationTransport(IHostBuilder hostBuilder)
        {
            var inMemoryIntegrationTransportAssembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(
                    nameof(SpaceEngineers),
                    nameof(Core),
                    nameof(Core.IntegrationTransport),
                    nameof(Core.IntegrationTransport.InMemory)));

            var endpointInstanceSelectionBehavior = new EndpointInstanceSelectionBehavior();
            var inMemoryIntegrationTransport = new InMemoryIntegrationTransport(endpointInstanceSelectionBehavior);
            var inMemoryIntegrationTransportManualRegistration = new InMemoryIntegrationTransportManualRegistration(inMemoryIntegrationTransport, endpointInstanceSelectionBehavior);

            var inMemoryIntegrationTransportModifier = new Func<DependencyContainerOptions, DependencyContainerOptions>(options => options.WithManualRegistrations(inMemoryIntegrationTransportManualRegistration));

            EndpointPluginAssemblies = EndpointPluginAssemblies
               .Concat(new[] { inMemoryIntegrationTransportAssembly })
               .ToList();

            Modifiers = Modifiers
               .Concat(new[] { inMemoryIntegrationTransportModifier })
               .ToList();

            return this;
        }

        public ITransportEndpointBuilder WithStartupAction(Func<IDependencyContainer, IHostStartupAction> producer)
        {
            StartupActions = StartupActions
               .Concat(new[] { producer })
               .ToList();

            return this;
        }

        public ITransportEndpointBuilder WithBackgroundWorker(Func<IDependencyContainer, IHostBackgroundWorker> producer)
        {
            BackgroundWorkers = BackgroundWorkers
               .Concat(new[] { producer })
               .ToList();

            return this;
        }

        public TransportEndpointOptions BuildOptions()
        {
            var containerOptions = new DependencyContainerOptions();

            if (Modifiers.Any())
            {
                containerOptions = Modifiers.Aggregate(containerOptions, (current, modifier) => modifier(current));
            }

            return new TransportEndpointOptions(
                EndpointIdentity,
                containerOptions,
                _rootAssemblies.Concat(EndpointPluginAssemblies).ToArray());
        }
    }
}