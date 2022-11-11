namespace SpaceEngineers.Core.GenericDomain.EventSourcing.Sql
{
    using System;
    using DataAccess.Api.Model;
    using DataAccess.Api.Sql;
    using DataAccess.Api.Sql.Attributes;

    /// <summary>
    /// DatabaseDomainEvent
    /// </summary>
    [Schema(nameof(EventSourcing))]
    [Index(nameof(AggregateId), nameof(Index), Unique = true)]
    public record DatabaseDomainEvent : BaseDatabaseEntity<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="aggregateId">AggregateId</param>
        /// <param name="index">Index</param>
        /// <param name="timestamp">Timestamp</param>
        /// <param name="eventType">EventType</param>
        /// <param name="serializedEvent">SerializedEvent</param>
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

        /// <summary>
        /// AggregateId
        /// </summary>
        public Guid AggregateId { get; set; }

        /// <summary>
        /// Index
        /// </summary>
        public long Index { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// EventType
        /// </summary>
        public SystemType EventType { get; set; }

        /// <summary>
        /// SerializedEvent
        /// </summary>
        public string SerializedEvent { get; set; }
    }
}