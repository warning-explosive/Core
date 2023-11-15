namespace SpaceEngineers.Core.GenericEndpoint.UnitOfWork
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    /// IOutboxDelivery
    /// </summary>
    public interface IOutboxDelivery
    {
        /// <summary>
        /// Delivers messages
        /// </summary>
        /// <param name="messages">Integration messages</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task DeliverMessages(
            IReadOnlyCollection<IntegrationMessage> messages,
            CancellationToken token);
    }
}