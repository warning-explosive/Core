namespace SpaceEngineers.Core.AuthEndpoint.Host
{
    using System;
    using Basics;
    using Contract;
    using GenericEndpoint.Authorization.Host;
    using GenericEndpoint.Host;
    using GenericEndpoint.Host.Builder;
    using Microsoft.Extensions.Hosting;
    using SpaceEngineers.Core.GenericEndpoint.Contract;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// UseAuthEndpoint
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="optionsFactory">Endpoint options factory</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseAuthEndpoint(
            this IHostBuilder hostBuilder,
            Func<IEndpointBuilder, EndpointOptions> optionsFactory)
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(AuthEndpoint), nameof(Domain))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(AuthEndpoint), nameof(Contract))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(AuthEndpoint)))
            };

            return hostBuilder.UseEndpoint(
                new EndpointIdentity(Identity.LogicalName, Guid.NewGuid().ToString()),
                (context, endpointBuilder) => optionsFactory(endpointBuilder
                    .WithEndpointPluginAssemblies(assemblies)
                    .WithAuthorization(context.Configuration)));
        }
    }
}