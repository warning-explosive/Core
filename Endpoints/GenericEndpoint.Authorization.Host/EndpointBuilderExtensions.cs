namespace SpaceEngineers.Core.GenericEndpoint.Authorization.Host
{
    using Basics;
    using GenericEndpoint.Host.Builder;
    using JwtAuthentication;
    using Microsoft.Extensions.Configuration;
    using Registrations;

    /// <summary>
    /// EndpointBuilderExtensions
    /// </summary>
    public static class EndpointBuilderExtensions
    {
        /// <summary>
        /// With authorization
        /// </summary>
        /// <param name="builder">Endpoint builder</param>
        /// <param name="configuration">IConfiguration</param>
        /// <returns>IEndpointBuilder</returns>
        public static IEndpointBuilder WithAuthorization(
            this IEndpointBuilder builder,
            IConfiguration configuration)
        {
            var authorization = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericEndpoint), nameof(Authorization)))
            };

            return builder
                .WithEndpointPluginAssemblies(authorization)
                .ModifyContainerOptions(options => options.WithManualRegistrations(
                    new JwtSecurityTokenHandlerManualRegistration(),
                    new JwtAuthenticationConfigurationManualRegistration(configuration.GetJwtAuthenticationConfiguration())));
        }
    }
}