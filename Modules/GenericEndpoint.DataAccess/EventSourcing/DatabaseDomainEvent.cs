namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.EventSourcing
{
    using System;
    using Core.DataAccess.Api.Model;

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

        public Guid AggregateId { get; private init; }

        public long Index { get; private init; }

        public DateTime Timestamp { get; private init; }

        public SystemType EventType { get; private init; }

        public string SerializedEvent { get; private init; }
    }
}