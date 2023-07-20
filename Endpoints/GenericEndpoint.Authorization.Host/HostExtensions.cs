namespace SpaceEngineers.Core.GenericEndpoint.Authorization.Host
{
    using Basics;
    using GenericEndpoint.Host.Builder;
    using JwtAuthentication;
    using Microsoft.Extensions.Configuration;
    using Registrations;
    using SpaceEngineers.Core.CrossCuttingConcerns.Settings;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Gets JwtAuthenticationConfiguration
        /// </summary>
        /// <param name="configuration">IConfiguration</param>
        /// <returns>JwtAuthenticationConfiguration</returns>
        public static JwtAuthenticationConfiguration GetJwtAuthenticationConfiguration(
            this IConfiguration configuration)
        {
            var section = configuration.GetRequiredSection($"Endpoints:{AuthEndpoint.Contract.Identity.LogicalName}:Authorization");

            // var privateKey = Convert.ToBase64String(new HMACSHA256().Key)
            var issuer = section.GetRequiredValue<string>("Issuer");
            var audience = section.GetRequiredValue<string>("Audience");
            var privateKey = section.GetRequiredValue<string>("PrivateKey");

            return new JwtAuthenticationConfiguration(issuer, audience, privateKey);
        }

        /// <summary>
        /// With authorization
        /// </summary>
        /// <param name="builder">IEndpointBuilder</param>
        /// <param name="configuration">IConfiguration</param>
        /// <returns>Configured IEndpointBuilder</returns>
        public static IEndpointBuilder WithAuthorization(
            this IEndpointBuilder builder,
            IConfiguration configuration)
        {
            // TODO: builder.CheckDuplicates(); - widespread
            var assembly = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(JwtAuthentication))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericEndpoint), nameof(GenericEndpoint.Authorization)))
            };

            return builder
                .ModifyContainerOptions(options => options
                    .WithPluginAssemblies(assembly)
                    .WithManualRegistrations(
                        new JwtSecurityTokenHandlerManualRegistration(),
                        new JwtAuthenticationConfigurationManualRegistration(configuration.GetJwtAuthenticationConfiguration())));
        }
    }
}