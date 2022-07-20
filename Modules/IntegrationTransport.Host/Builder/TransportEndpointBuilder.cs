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
            var assembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(
                    nameof(SpaceEngineers),
                    nameof(Core),
                    nameof(Core.IntegrationTransport),
                    nameof(Core.IntegrationTransport.InMemory)));

            var endpointInstanceSelectionBehavior = new EndpointInstanceSelectionBehavior();
            var inMemoryIntegrationTransport = new InMemoryIntegrationTransport(EndpointIdentity, endpointInstanceSelectionBehavior);
            var inMemoryIntegrationTransportManualRegistration = new InMemoryIntegrationTransportManualRegistration(inMemoryIntegrationTransport, endpointInstanceSelectionBehavior);

            var modifier = new Func<DependencyContainerOptions, DependencyContainerOptions>(options => options.WithManualRegistrations(inMemoryIntegrationTransportManualRegistration));

            EndpointPluginAssemblies = EndpointPluginAssemblies
               .Concat(new[] { assembly })
               .ToList();

            Modifiers = Modifiers
               .Concat(new[] { modifier })
               .ToList();

            return this;
        }

        public ITransportEndpointBuilder WithRabbitMqIntegrationTransport(IHostBuilder hostBuilder)
        {
            var assembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(
                    nameof(SpaceEngineers),
                    nameof(Core),
                    nameof(Core.IntegrationTransport),
                    nameof(Core.IntegrationTransport.RabbitMQ)));

            var manualRegistration = new RabbitMqIntegrationTransportManualRegistration();

            var modifier = new Func<DependencyContainerOptions, DependencyContainerOptions>(options => options.WithManualRegistrations(manualRegistration));

            EndpointPluginAssemblies = EndpointPluginAssemblies
               .Concat(new[] { assembly })
               .ToList();

            Modifiers = Modifiers
               .Concat(new[] { modifier })
               .ToList();

            return this;
        }
    }
}