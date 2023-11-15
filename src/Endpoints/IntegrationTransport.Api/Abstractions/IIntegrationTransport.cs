namespace SpaceEngineers.Core.IntegrationTransport.Api.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;
    using Enumerations;
    using GenericEndpoint.Messaging;

    /// <summary>
    /// IIntegrationTransport
    /// </summary>
    public interface IIntegrationTransport
    {
        /// <summary>
        /// Status
        /// </summary>
        EnIntegrationTransportStatus Status { get; }

        /// <summary>
        /// Enqueue message into input queue
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Returns true if operation was finished successfully</returns>
        Task<bool> Enqueue(
            IntegrationMessage message,
            CancellationToken token);
    }
}