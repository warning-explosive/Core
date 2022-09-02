namespace SpaceEngineers.Core.IntegrationTransport.Api.Abstractions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IExecutableIntegrationTransport
    /// </summary>
    public interface IExecutableIntegrationTransport
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
        /// Starts message processing in background thread
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing message processing operation</returns>
        public Task StartBackgroundMessageProcessing(CancellationToken token);
    }
}