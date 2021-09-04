namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using Messaging;

    /// <summary>
    /// IOutboxStorage
    /// </summary>
    public interface IOutboxStorage : IResolvable
    {
        /// <summary>
        /// All messages are ready to be delivered
        /// </summary>
        IReadOnlyCollection<IntegrationMessage> All { get; }

        /// <summary>
        /// Add message to the storage
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Add(IntegrationMessage message, CancellationToken token);

        /// <summary>
        /// Ack that message has been delivered
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Ack(IntegrationMessage message, CancellationToken token);

        /// <summary>
        /// Purge outbox storage
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Purge(CancellationToken token);
    }
}