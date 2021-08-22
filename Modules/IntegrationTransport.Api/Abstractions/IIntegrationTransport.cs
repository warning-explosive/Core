namespace SpaceEngineers.Core.IntegrationTransport.Api.Abstractions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;

    /// <summary>
    /// IIntegrationTransport
    /// </summary>
    public interface IIntegrationTransport : IResolvable
    {
        /// <summary>
        /// Bind message handler and configure topology
        /// </summary>
        /// <param name="message">Message type</param>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <param name="messageHandler">Message handler</param>
        public void Bind(Type message, EndpointIdentity endpointIdentity, Func<IntegrationMessage, Task> messageHandler);

        /// <summary>
        /// Enqueue message into input queue
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Enqueue(IntegrationMessage message, CancellationToken token);

        /// <summary>
        /// Enqueue message into error queue
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="exception">Exception</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task EnqueueError(IntegrationMessage message, Exception exception, CancellationToken token);

        /// <summary>
        /// Starts message processing
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing message processing operation</returns>
        public Task StartMessageProcessing(CancellationToken token);
    }
}