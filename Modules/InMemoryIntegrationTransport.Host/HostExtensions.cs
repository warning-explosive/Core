namespace SpaceEngineers.Core.InMemoryIntegrationTransport.Host
{
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Basics;
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

        private static IDependencyContainer BuildTransportContainer(
            HostBuilderContext ctx,
            IServiceCollection serviceCollection)
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.IntegrationTransport))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.InMemoryIntegrationTransport))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.InMemoryIntegrationTransport), nameof(Core.InMemoryIntegrationTransport.Host))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.Messaging))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CrossCuttingConcerns)))
            };

            var containerOptions = new DependencyContainerOptions().WithManualRegistrations(new InMemoryIntegrationTransportManualRegistration());

            var dependencyContainer = DependencyContainer.CreateBoundedAbove(containerOptions, assemblies);

            var injection = dependencyContainer.Resolve<IManualRegistration>();

            ctx.Properties.Add(GenericHost.Api.HostExtensions.TransportInjectionKey, injection);

            var transport = dependencyContainer.Resolve<InMemoryIntegrationTransport>();

            serviceCollection.AddSingleton<IHostBackgroundWorker>(new IntegrationTransportHostBackgroundWorker(transport));

            return dependencyContainer;
        }
    }
}