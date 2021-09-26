namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using GenericDomain.Api.Abstractions;

    internal class InboxMessageWasMovedToErrorQueue : IDomainEvent
    {
        public InboxMessageWasMovedToErrorQueue(Guid inboxId, Guid messageId)
        {
            InboxId = inboxId;
            MessageId = messageId;
        }

        public Guid InboxId { get; }

        public Guid MessageId { get; }
    }
}