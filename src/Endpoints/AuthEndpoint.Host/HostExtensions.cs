namespace SpaceEngineers.Core.AuthEndpoint.Host
{
    using System;
    using Basics;
    using Contract;
    using GenericEndpoint.Host;
    using GenericEndpoint.Host.Builder;
    using Microsoft.Extensions.Hosting;

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
                Identity.EndpointIdentity,
                builder => optionsFactory(
                    builder
                        .ModifyContainerOptions(options => options
                            .WithPluginAssemblies(assemblies)
                            .WithAdditionalOurTypes(typeof(AuthController)))));
        }
    }
}