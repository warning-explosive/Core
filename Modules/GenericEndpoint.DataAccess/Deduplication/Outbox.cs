namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CrossCuttingConcerns.Api.Abstractions;
    using GenericDomain.Api.Abstractions;
    using IntegrationTransport.Api.Abstractions;
    using Messaging;

    internal class Outbox : BaseAggregate
    {
        public Outbox(IReadOnlyCollection<IntegrationMessage> outgoingMessages)
        {
            OutgoingMessages = outgoingMessages;

            if (outgoingMessages.Any())
            {
                PopulateEvent(new OutboxMessagesAreReadyToBeSent(Id, outgoingMessages));
            }
        }

        public Outbox(
            Guid primaryKey,
            IReadOnlyCollection<DatabaseModel.IntegrationMessage> outgoingMessages,
            IJsonSerializer serializer,
            IStringFormatter formatter)
        {
            Id = primaryKey;

            OutgoingMessages = outgoingMessages
                .Select(message => message.BuildIntegrationMessage(serializer, formatter))
                .ToList();
        }

        public IReadOnlyCollection<IntegrationMessage> OutgoingMessages { get; }

        public async Task DeliverMessages(
            IIntegrationTransport transport,
            CancellationToken token)
        {
            if (!OutgoingMessages.Any())
            {
                return;
            }

            foreach (var message in OutgoingMessages)
            {
                await transport.Enqueue(message, token).ConfigureAwait(false);
            }

            PopulateEvent(new OutboxMessagesHaveBeenSent(OutgoingMessages.Select(message => message.Id).ToArray()));
        }
    }
}