namespace SpaceEngineers.Core.StatisticsEndpoint.Host
{
    using System;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using GenericEndpoint.Host;
    using GenericEndpoint.Host.Abstractions;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Use specified endpoint
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="implementationProducer">Dependency container implementation producer</param>
        /// <param name="factory">Endpoint options factory</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseStatisticsEndpoint(
            this IHostBuilder hostBuilder,
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> implementationProducer,
            Func<IEndpointBuilder, EndpointOptions> factory)
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(SpaceEngineers.Core), nameof(Core.StatisticsEndpoint), nameof(Core.StatisticsEndpoint.Contract))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(SpaceEngineers.Core), nameof(Core.StatisticsEndpoint)))
            };

            return hostBuilder.UseEndpoint(
                implementationProducer,
                endpointBuilder => factory(endpointBuilder.WithEndpointPluginAssemblies(assemblies)));
        }
    }
}