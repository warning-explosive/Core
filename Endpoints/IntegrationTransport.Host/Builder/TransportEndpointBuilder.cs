﻿namespace SpaceEngineers.Core.IntegrationTransport.Host.Builder
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
    using Registrations;

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
                    nameof(IntegrationTransport),
                    nameof(InMemory)));

            var endpointInstanceSelectionBehavior = new EndpointInstanceSelectionBehavior();
            var inMemoryIntegrationTransport = new InMemoryIntegrationTransport(EndpointIdentity, endpointInstanceSelectionBehavior);
            var inMemoryIntegrationTransportManualRegistration = new InMemoryIntegrationTransportManualRegistration(inMemoryIntegrationTransport, endpointInstanceSelectionBehavior);
            var rpcRequestRegistryManualRegistration = new RpcRequestRegistryManualRegistration();

            var modifier = new Func<DependencyContainerOptions, DependencyContainerOptions>(options => options
               .WithManualRegistrations(inMemoryIntegrationTransportManualRegistration, rpcRequestRegistryManualRegistration));

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
                    nameof(IntegrationTransport),
                    nameof(RabbitMQ)));

            var rabbitMqIntegrationTransportManualRegistration = new RabbitMqIntegrationTransportManualRegistration();
            var rpcRequestRegistryManualRegistration = new RpcRequestRegistryManualRegistration();

            var modifier = new Func<DependencyContainerOptions, DependencyContainerOptions>(options => options
               .WithManualRegistrations(rabbitMqIntegrationTransportManualRegistration, rpcRequestRegistryManualRegistration));

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