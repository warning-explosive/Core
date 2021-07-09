namespace SpaceEngineers.Core.InMemoryIntegrationTransport.Endpoint.Host
{
    using Basics;
    using GenericEndpoint.Host.Abstractions;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Use in-memory integration transport inside specified endpoint
        /// </summary>
        /// <param name="endpointBuilder">IEndpointBuilder</param>
        /// <returns>Configured IEndpointBuilder</returns>
        public static IEndpointBuilder WithInMemoryIntegrationTransport(this IEndpointBuilder endpointBuilder)
        {
            var assembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.InMemoryIntegrationTransport), nameof(Core.InMemoryIntegrationTransport.Endpoint)));

            return endpointBuilder.WithEndpointPluginAssemblies(assembly);
        }
    }
}