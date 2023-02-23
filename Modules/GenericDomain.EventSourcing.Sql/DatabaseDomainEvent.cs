namespace SpaceEngineers.Core.GenericDomain.EventSourcing.Sql
{
    using System;
    using Api.Abstractions;
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
        /// <param name="domainEvent">IDomainEvent</param>
        public DatabaseDomainEvent(
            Guid primaryKey,
            Guid aggregateId,
            long index,
            DateTime timestamp,
            IDomainEvent domainEvent)
            : base(primaryKey)
        {
            AggregateId = aggregateId;
            Index = index;
            Timestamp = timestamp;
            DomainEvent = domainEvent;
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
        /// Domain event
        /// </summary>
        [JsonColumn]
        public IDomainEvent DomainEvent { get; set; }
    }
}