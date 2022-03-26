namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using GenericDomain.Api.Abstractions;

    internal class OutboxMessagesHaveBeenSent : IDomainEvent
    {
        public OutboxMessagesHaveBeenSent(Guid[] messageIds)
        {
            MessageIds = messageIds;
        }

        public Guid[] MessageIds { get; }
    }
}