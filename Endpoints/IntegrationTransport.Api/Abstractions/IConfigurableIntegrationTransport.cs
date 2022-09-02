namespace SpaceEngineers.Core.IntegrationTransport.Api.Abstractions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;

    /// <summary>
    /// IConfigurableIntegrationTransport
    /// </summary>
    public interface IConfigurableIntegrationTransport
    {
        /// <summary>
        /// Bind message handler and configure topology
        /// </summary>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <param name="messageHandler">Message handler</param>
        /// <param name="integrationTypeProvider">IIntegrationTypeProvider</param>
        public void Bind(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, Task> messageHandler,
            IIntegrationTypeProvider integrationTypeProvider);

        /// <summary>
        /// Bind message handler for error messages
        /// </summary>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <param name="errorMessageHandler">Error message handler</param>
        public void BindErrorHandler(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, Exception, CancellationToken, Task> errorMessageHandler);
    }
}