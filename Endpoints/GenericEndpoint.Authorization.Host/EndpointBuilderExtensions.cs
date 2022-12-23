namespace SpaceEngineers.Core.GenericEndpoint.Authorization.Host
{
    using Basics;
    using GenericEndpoint.Host.Builder;

    /// <summary>
    /// EndpointBuilderExtensions
    /// </summary>
    public static class EndpointBuilderExtensions
    {
        /// <summary>
        /// With authorization
        /// </summary>
        /// <param name="builder">Endpoint builder</param>
        /// <returns>IEndpointBuilder</returns>
        public static IEndpointBuilder WithAuthorization(
            this IEndpointBuilder builder)
        {
            var authorization = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericEndpoint), nameof(Authorization)))
            };

            return builder.WithEndpointPluginAssemblies(authorization);
        }
    }
}