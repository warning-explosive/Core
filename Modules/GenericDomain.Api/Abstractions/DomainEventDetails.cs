namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System;

    /// <summary>
    /// DomainEventDetails
    /// </summary>
    public class DomainEventDetails
    {
        /// <summary> .cctor </summary>
        /// <param name="aggregateId">AggregateId</param>
        /// <param name="index">Index</param>
        /// <param name="timestamp">Timestamp</param>
        public DomainEventDetails(
            Guid aggregateId,
            long index,
            DateTime timestamp)
        {
            AggregateId = aggregateId;
            Index = index;
            Timestamp = timestamp;
        }

        /// <summary>
        /// AggregateId
        /// </summary>
        public Guid AggregateId { get; init; }

        /// <summary>
        /// Index
        /// </summary>
        public long Index { get; init; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; init; }
    }
}