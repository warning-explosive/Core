namespace SpaceEngineers.Core.GenericEndpoint.Authorization.Web.Host
{
    using System.Net;
    using Basics;
    using GenericEndpoint.Host.Builder;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// With web authorization
        /// </summary>
        /// <param name="builder">IEndpointBuilder</param>
        /// <returns>Configured IEndpointBuilder</returns>
        public static IEndpointBuilder WithWebAuthorization(this IEndpointBuilder builder)
        {
            builder.CheckMultipleCalls(nameof(WithWebAuthorization));

            var assembly = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericEndpoint), nameof(Authorization), nameof(Web)))
            };

            return builder
                .ModifyContainerOptions(options => options
                    .WithPluginAssemblies(assembly));
        }
    }
}