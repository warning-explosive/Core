namespace SpaceEngineers.Core.InMemoryIntegrationTransport.Host
{
    using System;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericHost.Api;
    using GenericHost.Api.Abstractions;
    using Internals;
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
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseInMemoryIntegrationTransport(this IHostBuilder hostBuilder)
        {
            hostBuilder.CheckMultipleCalls(nameof(UseInMemoryIntegrationTransport));

            return hostBuilder.ConfigureServices((ctx, serviceCollection) =>
            {
                serviceCollection.AddSingleton<IDependencyContainer>(BuildTransportContainer(ctx, serviceCollection));
            });
        }

        private static IDependencyContainer BuildTransportContainer(HostBuilderContext context, IServiceCollection serviceCollection)
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.IntegrationTransport))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.InMemoryIntegrationTransport))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.InMemoryIntegrationTransport), nameof(Core.InMemoryIntegrationTransport.Host))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.Messaging))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CrossCuttingConcerns)))
            };

            var endpointIdentity = new EndpointIdentity(nameof(InMemoryIntegrationTransport), 0);

            var containerOptions = new DependencyContainerOptions()
                .WithManualRegistrations(new InMemoryIntegrationTransportManualRegistration())
                .WithManualRegistrations(new TransportEndpointIdentityManualRegistration(endpointIdentity));

            var containerImplementationProducer = GetContainerImplementationProducer(context);

            var dependencyContainer = DependencyContainer.CreateBoundedAbove(
                containerOptions,
                containerImplementationProducer(containerOptions),
                assemblies);

            var injection = dependencyContainer.Resolve<IManualRegistration>();

            context.Properties.Add(GenericHost.Api.HostExtensions.TransportInjectionKey, injection);

            var transport = dependencyContainer.Resolve<InMemoryIntegrationTransport>();

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