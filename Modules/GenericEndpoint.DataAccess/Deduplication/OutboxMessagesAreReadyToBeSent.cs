namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System.Collections.Generic;
    using GenericDomain.Api.Abstractions;
    using Messaging;

    internal class OutboxMessagesAreReadyToBeSent : IDomainEvent
    {
        public OutboxMessagesAreReadyToBeSent(IReadOnlyCollection<IntegrationMessage> outgoingMessages)
        {
            OutgoingMessages = outgoingMessages;
        }

        public IReadOnlyCollection<IntegrationMessage> OutgoingMessages { get; }
    }
}