namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using System;
    using GenericDomain.Api.Abstractions;

    internal class OutboxAggregateSpecification : IAggregateSpecification
    {
        public OutboxAggregateSpecification(Guid outboxId)
        {
            OutboxId = outboxId;
        }

        public Guid OutboxId { get; }
    }
}