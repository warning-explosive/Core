namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using GenericDomain.Api.Abstractions;

    internal class InboxMessageWasMovedToErrorQueue : IDomainEvent
    {
        public InboxMessageWasMovedToErrorQueue(Guid messageId)
        {
            MessageId = messageId;
        }

        public Guid MessageId { get; }
    }
}