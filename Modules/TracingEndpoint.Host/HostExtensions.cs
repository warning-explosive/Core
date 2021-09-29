namespace SpaceEngineers.Core.TracingEndpoint.Host
{
    using System;
    using Basics;
    using GenericEndpoint.Host;
    using GenericEndpoint.Host.Builder;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// UseTracingEndpoint
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="factory">Endpoint options factory</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseTracingEndpoint(
            this IHostBuilder hostBuilder,
            Func<IEndpointBuilder, EndpointOptions> factory)
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(SpaceEngineers.Core), nameof(Core.TracingEndpoint), nameof(Core.TracingEndpoint.Contract))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(SpaceEngineers.Core), nameof(Core.TracingEndpoint))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(SpaceEngineers.Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.Tracing)))
            };

            return hostBuilder.UseEndpoint(endpointBuilder => factory(endpointBuilder.WithEndpointPluginAssemblies(assemblies)));
        }
    }
}