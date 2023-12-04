namespace SpaceEngineers.Core.GenericEndpoint.UnitOfWork
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    /// ITransactionalOutbox
    /// </summary>
    public interface ITransactionalOutbox
    {
        /// <summary>
        /// Adds message to the outbox storage
        /// </summary>
        /// <param name="message">Integration message</param>
        void Add(IntegrationMessage message);

        /// <summary>
        /// Gets all outgoing messages that are ready to be delivered
        /// </summary>
        /// <returns>Ongoing operation</returns>
        IReadOnlyCollection<IntegrationMessage> All();

        /// <summary>
        /// Delivers messages
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task DeliverMessages(CancellationToken token);
    }
}