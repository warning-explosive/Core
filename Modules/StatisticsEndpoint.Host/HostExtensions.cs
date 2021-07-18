namespace SpaceEngineers.Core.StatisticsEndpoint.Host
{
    using System;
    using Basics;
    using GenericEndpoint;
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
        /// <param name="factory">Endpoint options factory</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseStatisticsEndpoint(this IHostBuilder hostBuilder, Func<IEndpointBuilder, EndpointOptions> factory)
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(SpaceEngineers.Core), nameof(Core.StatisticsEndpoint), nameof(Core.StatisticsEndpoint.Contract))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(SpaceEngineers.Core), nameof(Core.StatisticsEndpoint)))
            };

            return hostBuilder.UseEndpoint(endpointBuilder => factory(endpointBuilder.WithEndpointPluginAssemblies(assemblies)));
        }
    }
}