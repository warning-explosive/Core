namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using Contract;
    using GenericDomain.Api.Abstractions;
    using Messaging;

    internal class InboxMessageReceived : IDomainEvent
    {
        public InboxMessageReceived(
            Guid inboxId,
            IntegrationMessage message,
            EndpointIdentity endpointIdentity)
        {
            InboxId = inboxId;
            Message = message;
            EndpointIdentity = endpointIdentity;
        }

        public Guid InboxId { get; }

        public IntegrationMessage Message { get; }

        public EndpointIdentity EndpointIdentity { get; }
    }
}