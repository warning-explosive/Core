namespace SpaceEngineers.Core.AuthorizationEndpoint.Host
{
    using System;
    using Basics;
    using Contract;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Host;
    using GenericEndpoint.Host.Builder;
    using JwtAuthentication;
    using ManualRegistrations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// UseAuthorizationEndpoint
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="endpointInstanceName">Endpoint instance name</param>
        /// <param name="optionsFactory">Endpoint options factory</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseAuthorizationEndpoint(
            this IHostBuilder hostBuilder,
            object endpointInstanceName,
            Func<IEndpointBuilder, EndpointOptions> optionsFactory)
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(SpaceEngineers.Core), nameof(Core.AuthorizationEndpoint), nameof(Core.AuthorizationEndpoint.Contract))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(SpaceEngineers.Core), nameof(Core.AuthorizationEndpoint)))
            };

            return hostBuilder.UseEndpoint(
                new EndpointIdentity(AuthorizationEndpointIdentity.LogicalName, endpointInstanceName),
                (context, endpointBuilder) => optionsFactory(endpointBuilder
                    .WithEndpointPluginAssemblies(assemblies)
                    .ModifyContainerOptions(options => options.WithManualRegistrations(new JwtTokenProviderManualRegistration(context.Configuration.GetAuthenticationConfiguration())))));
        }

        internal static JwtAuthenticationConfiguration GetAuthenticationConfiguration(this IConfiguration configuration)
        {
            var section = configuration.GetRequiredSection("Authorization");

            var issuer = section["Issuer"];
            var audience = section["Audience"];
            var privateKey = section["PrivateKey"];

            return new JwtAuthenticationConfiguration(issuer, audience, privateKey);
        }
    }
}