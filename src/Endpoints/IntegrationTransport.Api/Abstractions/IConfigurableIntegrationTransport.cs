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
        /// Binds message handler
        /// </summary>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <param name="messageHandler">Message handler</param>
        /// <param name="integrationTypeProvider">IIntegrationTypeProvider</param>
        void BindMessageHandler(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, Task> messageHandler,
            IIntegrationTypeProvider integrationTypeProvider);

        /// <summary>
        /// Binds error handler
        /// </summary>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <param name="errorMessageHandler">Error message handler</param>
        void BindErrorHandler(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, Exception, CancellationToken, Task> errorMessageHandler);

        /// <summary>
        /// Locks topology configuration for specified endpoint
        /// </summary>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        void LockTopologyConfiguration(EndpointIdentity endpointIdentity);
    }
}