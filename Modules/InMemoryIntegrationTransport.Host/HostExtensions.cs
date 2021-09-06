namespace SpaceEngineers.Core.InMemoryIntegrationTransport.Host
{
    using System;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using CompositionRoot.Api.Abstractions.Registration;
    using GenericEndpoint.Contract;
    using GenericHost.Api;
    using GenericHost.Api.Abstractions;
    using Implementations;
    using IntegrationTransport.Api.Abstractions;
    using ManualRegistrations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        private const string RequireUseContainerCall = ".UseContainer() should be called before any endpoint declarations";

        /// <summary>
        /// Use in-memory integration transport inside specified host
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="modifier">Optional transport dependency container options modifier</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseInMemoryIntegrationTransport(
            this IHostBuilder hostBuilder,
            Func<DependencyContainerOptions, DependencyContainerOptions>? modifier = null)
        {
            hostBuilder.CheckMultipleCalls(nameof(UseInMemoryIntegrationTransport));

            return hostBuilder.ConfigureServices((ctx, serviceCollection) =>
            {
                serviceCollection.AddSingleton<IDependencyContainer>(BuildTransportContainer(ctx, serviceCollection, modifier));
            });
        }

        private static IDependencyContainer BuildTransportContainer(
            HostBuilderContext context,
            IServiceCollection serviceCollection,
            Func<DependencyContainerOptions, DependencyContainerOptions>? modifier)
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.IntegrationTransport))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.InMemoryIntegrationTransport))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.InMemoryIntegrationTransport), nameof(Core.InMemoryIntegrationTransport.Host))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.Messaging))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CrossCuttingConcerns)))
            };

            var endpointIdentity = new EndpointIdentity(nameof(InMemoryIntegrationTransport), Guid.NewGuid());

            var options = new DependencyContainerOptions()
                .WithManualRegistrations(new InMemoryIntegrationTransportManualRegistration())
                .WithManualRegistrations(new TransportEndpointIdentityManualRegistration(endpointIdentity));

            if (modifier != null)
            {
                options = modifier(options);
            }

            var containerImplementationProducer = GetContainerImplementationProducer(context);

            var dependencyContainer = DependencyContainer.CreateBoundedAbove(
                options,
                containerImplementationProducer(options),
                assemblies);

            var injection = dependencyContainer.Resolve<IManualRegistration>();

            context.Properties.Add(GenericHost.Api.HostExtensions.TransportInjectionKey, injection);

            var transport = dependencyContainer.Resolve<IIntegrationTransport>();

            serviceCollection.AddSingleton<IHostBackgroundWorker>(new IntegrationTransportHostBackgroundWorker(transport));

            return dependencyContainer;
        }

        private static Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> GetContainerImplementationProducer(HostBuilderContext context)
        {
            if (context.Properties.TryGetValue(nameof(IDependencyContainerImplementation), out var value)
                && value is Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> containerImplementationProducer)
            {
                return containerImplementationProducer;
            }

            throw new InvalidOperationException(RequireUseContainerCall);
        }
    }
}