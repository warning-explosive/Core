namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using GenericDomain.Api.Abstractions;

    internal class InboxMessageWasHandled : IDomainEvent
    {
        public InboxMessageWasHandled(Guid inboxId)
        {
            InboxId = inboxId;
        }

        public Guid InboxId { get; }
    }
}