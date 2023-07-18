namespace SpaceEngineers.Core.IntegrationTransport.Host.Builder
{
    using GenericEndpoint.Host.Builder;

    /// <summary>
    /// ITransportEndpointBuilder
    /// </summary>
    public interface ITransportEndpointBuilder : IEndpointBuilder
    {
        /// <summary>
        /// With in-memory integration transport
        /// </summary>
        /// <returns>ITransportEndpointBuilder</returns>
        public ITransportEndpointBuilder WithInMemoryIntegrationTransport();

        /// <summary>
        /// With RabbitMq integration transport
        /// </summary>
        /// <returns>ITransportEndpointBuilder</returns>
        public ITransportEndpointBuilder WithRabbitMqIntegrationTransport();
    }
}