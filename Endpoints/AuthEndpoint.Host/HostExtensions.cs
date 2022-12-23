namespace SpaceEngineers.Core.AuthEndpoint.Host
{
    using System;
    using Basics;
    using Contract;
    using GenericEndpoint.Host;
    using GenericEndpoint.Host.Builder;
    using JwtAuthentication;
    using Microsoft.Extensions.Hosting;
    using Registrations;
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
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.AuthEndpoint), nameof(Core.AuthEndpoint.Domain))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.AuthEndpoint), nameof(Core.AuthEndpoint.Contract))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.AuthEndpoint)))
            };

            return hostBuilder.UseEndpoint(
                new EndpointIdentity(AuthEndpointIdentity.LogicalName, Guid.NewGuid().ToString()),
                (_, endpointBuilder) => optionsFactory(endpointBuilder
                    .WithEndpointPluginAssemblies(assemblies)
                    .ModifyContainerOptions(options => options.WithManualRegistrations(
                            new JwtSecurityTokenHandlerManualRegistration(),
                            new JwtAuthenticationConfigurationManualRegistration(JwtAuthentication.HostExtensions.GetAuthEndpointConfiguration().GetJwtAuthenticationConfiguration())))));
        }
    }
}