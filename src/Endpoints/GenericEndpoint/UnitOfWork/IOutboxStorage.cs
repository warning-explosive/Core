namespace SpaceEngineers.Core.GenericEndpoint.UnitOfWork
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    /// IOutboxStorage
    /// </summary>
    public interface IOutboxStorage
    {
        /// <summary>
        /// Adds message to the outbox storage
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Add(IntegrationMessage message, CancellationToken token);

        /// <summary>
        /// Gets all outgoing messages that are ready to be delivered
        /// </summary>
        /// <returns>Ongoing operation</returns>
        IReadOnlyCollection<IntegrationMessage> All();

        /// <summary>
        /// Clears outbox storage
        /// </summary>
        void Clear();
    }
}