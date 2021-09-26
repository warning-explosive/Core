namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using GenericDomain.Api.Abstractions;

    internal class InboxMessageWasHandled : IDomainEvent
    {
        public InboxMessageWasHandled(Guid inboxId, Guid messageId)
        {
            InboxId = inboxId;
            MessageId = messageId;
        }

        public Guid InboxId { get; }

        public Guid MessageId { get; }
    }
}