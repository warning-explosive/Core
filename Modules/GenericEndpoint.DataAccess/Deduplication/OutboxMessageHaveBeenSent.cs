namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using GenericDomain.Api.Abstractions;

    internal class OutboxMessageHaveBeenSent : IDomainEvent
    {
        public OutboxMessageHaveBeenSent(Guid messageId)
        {
            MessageId = messageId;
        }

        public Guid MessageId { get; }
    }
}