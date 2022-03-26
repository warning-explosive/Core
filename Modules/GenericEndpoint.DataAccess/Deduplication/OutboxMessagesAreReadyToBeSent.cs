namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Collections.Generic;
    using GenericDomain.Api.Abstractions;
    using Messaging;

    internal class OutboxMessagesAreReadyToBeSent : IDomainEvent
    {
        public OutboxMessagesAreReadyToBeSent(
            Guid outboxId,
            IReadOnlyCollection<IntegrationMessage> outgoingMessages)
        {
            OutboxId = outboxId;
            OutgoingMessages = outgoingMessages;
        }

        public Guid OutboxId { get; }

        public IReadOnlyCollection<IntegrationMessage> OutgoingMessages { get; }
    }
}