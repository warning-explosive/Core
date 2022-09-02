namespace SpaceEngineers.Core.IntegrationTransport.Host.Builder
{
    using GenericEndpoint.Host.Builder;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// ITransportEndpointBuilder
    /// </summary>
    public interface ITransportEndpointBuilder : IEndpointBuilder
    {
        /// <summary>
        /// With in-memory integration transport
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <returns>ITransportEndpointBuilder</returns>
        public ITransportEndpointBuilder WithInMemoryIntegrationTransport(IHostBuilder hostBuilder);

        /// <summary>
        /// With RabbitMq integration transport
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <returns>ITransportEndpointBuilder</returns>
        public ITransportEndpointBuilder WithRabbitMqIntegrationTransport(IHostBuilder hostBuilder);
    }
}