namespace SpaceEngineers.Core.IntegrationTransport.Host.Builder
{
    using System;
    using System.Linq;
    using Basics;
    using CompositionRoot;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Host.Builder;
    using InMemory;
    using InMemory.Registrations;
    using Microsoft.Extensions.Hosting;
    using RabbitMQ.Registrations;

    internal class TransportEndpointBuilder : EndpointBuilder, ITransportEndpointBuilder
    {
        internal TransportEndpointBuilder(EndpointIdentity endpointIdentity)
            : base(endpointIdentity)
        {
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
            var inMemoryIntegrationTransport = new InMemoryIntegrationTransport(EndpointIdentity, endpointInstanceSelectionBehavior);
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

        public ITransportEndpointBuilder WithRabbitMqIntegrationTransport(IHostBuilder hostBuilder)
        {
            var inMemoryIntegrationTransportAssembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(
                    nameof(SpaceEngineers),
                    nameof(Core),
                    nameof(Core.IntegrationTransport),
                    nameof(Core.IntegrationTransport.RabbitMQ)));

            var inMemoryIntegrationTransportManualRegistration = new RabbitMqIntegrationTransportManualRegistration();

            var inMemoryIntegrationTransportModifier = new Func<DependencyContainerOptions, DependencyContainerOptions>(options => options.WithManualRegistrations(inMemoryIntegrationTransportManualRegistration));

            EndpointPluginAssemblies = EndpointPluginAssemblies
               .Concat(new[] { inMemoryIntegrationTransportAssembly })
               .ToList();

            Modifiers = Modifiers
               .Concat(new[] { inMemoryIntegrationTransportModifier })
               .ToList();

            return this;
        }
    }
}