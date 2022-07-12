namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.EventSourcing
{
    using System;
    using Core.DataAccess.Api.Model;

    [Schema(nameof(EventSourcing))]
    [Index(nameof(AggregateId), nameof(Index), Unique = true)]
    internal record DatabaseDomainEvent : BaseDatabaseEntity<Guid>
    {
        public DatabaseDomainEvent(
            Guid primaryKey,
            Guid aggregateId,
            long index,
            DateTime timestamp,
            SystemType eventType,
            string serializedEvent)
            : base(primaryKey)
        {
            AggregateId = aggregateId;
            Index = index;
            Timestamp = timestamp;
            EventType = eventType;
            SerializedEvent = serializedEvent;
        }

        public Guid AggregateId { get; init; }

        public long Index { get; init; }

        public DateTime Timestamp { get; init; }

        public SystemType EventType { get; init; }

        public string SerializedEvent { get; init; }
    }
}