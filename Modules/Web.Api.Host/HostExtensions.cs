namespace SpaceEngineers.Core.Web.Api.Host
{
    using Basics;
    using IntegrationTransport.Host.Builder;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Adds Web.Api as plugin assembly
        /// </summary>
        /// <param name="builder">Transport endpoint builder</param>
        /// <returns>ITransportEndpointBuilder</returns>
        public static ITransportEndpointBuilder WithWebApi(this ITransportEndpointBuilder builder)
        {
            var assembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(
                    nameof(SpaceEngineers),
                    nameof(SpaceEngineers.Core),
                    nameof(SpaceEngineers.Core.Web),
                    nameof(SpaceEngineers.Core.Web.Api)));

            return builder.WithEndpointPluginAssemblies(assembly);
        }
    }
}