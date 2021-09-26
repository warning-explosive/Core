namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Core.DataAccess.Api.Transaction;
    using GenericDomain.Api.Abstractions;
    using IntegrationTransport.Api.Abstractions;
    using Messaging;

    /// <summary>
    /// Outbox
    /// </summary>
    public class Outbox : BaseAggregate
    {
        /// <summary> .cctor </summary>
        /// <param name="outgoingMessages">Subsequent integration messages</param>
        public Outbox(IReadOnlyCollection<IntegrationMessage> outgoingMessages)
            : this(outgoingMessages, new OutboxMessagesAreReadyToBeSent(outgoingMessages))
        {
        }

        private Outbox(IReadOnlyCollection<IntegrationMessage> outgoingMessages, IDomainEvent domainEvent)
        {
            OutgoingMessages = outgoingMessages;
            PopulateEvent(domainEvent);
        }

        /// <summary>
        /// Subsequent integration messages
        /// </summary>
        public IReadOnlyCollection<IntegrationMessage> OutgoingMessages { get; }

        /// <summary>
        /// Delivers outgoing messages Messages
        /// </summary>
        /// <param name="outgoingMessages">Outgoing integration messages</param>
        /// <param name="transport">IIntegrationTransport</param>
        /// <param name="transaction">IDatabaseTransaction</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        public static async Task DeliverMessages(
            IEnumerable<IntegrationMessage> outgoingMessages,
            IIntegrationTransport transport,
            IDatabaseTransaction transaction,
            CancellationToken token)
        {
            foreach (var message in outgoingMessages)
            {
                await transport.Enqueue(message, token).ConfigureAwait(false);

                await using (await transaction.Open(true, token).ConfigureAwait(false))
                {
                    var outbox = new Outbox(new[] { message }, new OutboxMessageHaveBeenSent(message.Id));

                    await transaction.Track(outbox, token).ConfigureAwait(false);
                }
            }
        }
    }
}