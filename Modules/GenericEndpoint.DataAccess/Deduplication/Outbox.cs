namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Contract;
    using CrossCuttingConcerns.Json;
    using GenericDomain.Api.Abstractions;
    using IntegrationTransport.Api.Abstractions;
    using Messaging;
    using Messaging.MessageHeaders;

    internal class Outbox : BaseAggregate
    {
        public Outbox(
            EndpointIdentity endpointIdentity,
            IReadOnlyCollection<IntegrationMessage> outgoingMessages)
        {
            OutgoingMessages = outgoingMessages;

            if (outgoingMessages.Any())
            {
                PopulateEvent(new OutboxMessagesAreReadyToBeSent(Id, endpointIdentity, outgoingMessages));
            }
        }

        public Outbox(
            Guid primaryKey,
            IReadOnlyCollection<DatabaseModel.IntegrationMessage> outgoingMessages,
            IJsonSerializer serializer)
        {
            Id = primaryKey;

            OutgoingMessages = outgoingMessages
                .Select(message => message.BuildIntegrationMessage(serializer))
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

            var messageIds = OutgoingMessages
               .Select(message => message.ReadRequiredHeader<Id>().Value)
               .ToArray();

            PopulateEvent(new OutboxMessagesHaveBeenSent(messageIds));
        }
    }
}