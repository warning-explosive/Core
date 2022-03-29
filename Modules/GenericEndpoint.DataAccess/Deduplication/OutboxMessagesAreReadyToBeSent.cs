namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using System.Collections.Generic;
    using Contract;
    using GenericDomain.Api.Abstractions;
    using Messaging;

    internal class OutboxMessagesAreReadyToBeSent : IDomainEvent
    {
        public OutboxMessagesAreReadyToBeSent(
            Guid outboxId,
            EndpointIdentity endpointIdentity,
            IReadOnlyCollection<IntegrationMessage> outgoingMessages)
        {
            OutboxId = outboxId;
            EndpointIdentity = endpointIdentity;
            OutgoingMessages = outgoingMessages;
        }

        public Guid OutboxId { get; }

        public EndpointIdentity EndpointIdentity { get; }

        public IReadOnlyCollection<IntegrationMessage> OutgoingMessages { get; }
    }
}