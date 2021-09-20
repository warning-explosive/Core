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
        /// <param name="initiator">Initiator integration message</param>
        /// <param name="subsequentMessages">Subsequent integration messages</param>
        public Outbox(
            IntegrationMessage initiator,
            IReadOnlyCollection<IntegrationMessage> subsequentMessages)
        {
            Initiator = initiator;
            SubsequentMessages = subsequentMessages;
            PopulateEvent(new OutboxMessagesAreReadyToBeSent());
        }

        /// <summary>
        /// Initiator integration message
        /// </summary>
        public IntegrationMessage Initiator { get; }

        /// <summary>
        /// Subsequent integration messages
        /// </summary>
        public IReadOnlyCollection<IntegrationMessage> SubsequentMessages { get; }

        /// <summary>
        /// Have subsequent integration messages been sent
        /// </summary>
        public bool Sent { get; private set; }

        /// <summary>
        /// Marks subsequent integration messages as sent
        /// </summary>
        /// <param name="integrationMessage">Integration message</param>
        public void MarkAsSent(IntegrationMessage integrationMessage)
        {
            Sent = true;
            PopulateEvent(new OutboxMessageWereSent(integrationMessage));
        }

        /// <summary>
        /// Delivers outgoing messages Messages
        /// </summary>
        /// <param name="transport">IIntegrationTransport</param>
        /// <param name="transaction">IDatabaseTransaction</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        public async Task DeliverMessages(
            IIntegrationTransport transport,
            IDatabaseTransaction transaction,
            CancellationToken token)
        {
            foreach (var message in SubsequentMessages)
            {
                await transport.Enqueue(message, token).ConfigureAwait(false);

                MarkAsSent(message);

                await using (await transaction.Open(true, token).ConfigureAwait(false))
                {
                    await transaction.Track(this, token).ConfigureAwait(false);
                }
            }
        }
    }
}