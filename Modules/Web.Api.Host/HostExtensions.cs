namespace SpaceEngineers.Core.Web.Api.Host
{
    using Basics;
    using GenericHost;
    using IntegrationTransport.Host.Builder;
    using JwtAuthentication;
    using Microsoft.Extensions.Hosting;
    using Registrations;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Adds Web.Api as plugin assembly
        /// </summary>
        /// <param name="builder">Transport endpoint builder</param>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <returns>ITransportEndpointBuilder</returns>
        public static ITransportEndpointBuilder WithWebApi(
            this ITransportEndpointBuilder builder,
            IHostBuilder hostBuilder)
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Web), nameof(SpaceEngineers.Core.Web.Api))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Web), nameof(Auth)))
            };

            var frameworkDependenciesProvider = hostBuilder.GetFrameworkDependenciesProvider();

            return (ITransportEndpointBuilder)builder
               .WithEndpointPluginAssemblies(assemblies)
               .ModifyContainerOptions(options => options.WithManualRegistrations(
                       new HttpContextAccessorManualRegistration(frameworkDependenciesProvider),
                       new JwtSecurityTokenHandlerManualRegistration(),
                       new JwtAuthenticationConfigurationManualRegistration(JwtAuthentication.HostExtensions.GetAuthEndpointConfiguration().GetJwtAuthenticationConfiguration())));
        }
    }
}