namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    /// IMessagesCollector
    /// </summary>
    public interface IMessagesCollector
    {
        /// <summary>
        /// IMessagesCollector
        /// </summary>
        /// <param name="message">IntegrationMessage</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Collect(IntegrationMessage message, CancellationToken token);
    }
}