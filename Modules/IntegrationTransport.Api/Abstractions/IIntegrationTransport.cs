namespace SpaceEngineers.Core.IntegrationTransport.Api.Abstractions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Enumerations;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.Abstractions;

    /// <summary>
    /// IIntegrationTransport
    /// </summary>
    public interface IIntegrationTransport
    {
        /// <summary>
        /// Status changed
        /// </summary>
        event EventHandler<IntegrationTransportStatusChangedEventArgs>? StatusChanged;

        /// <summary>
        /// Message received
        /// </summary>
        event EventHandler<IntegrationTransportMessageReceivedEventArgs>? MessageReceived;

        /// <summary>
        /// Status
        /// </summary>
        EnIntegrationTransportStatus Status { get; }

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

        /// <summary>
        /// Enqueue message into input queue
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Returns true if operation was finished successfully</returns>
        Task<bool> Enqueue(
            IntegrationMessage message,
            CancellationToken token);

        /// <summary>
        /// Starts message processing in background thread
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing message processing operation</returns>
        public Task StartBackgroundMessageProcessing(CancellationToken token);
    }
}