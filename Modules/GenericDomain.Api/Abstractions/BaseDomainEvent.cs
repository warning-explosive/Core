namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System;

    /// <summary>
    /// BaseDomainEvent
    /// </summary>
    /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
    /// <typeparam name="TEvent">TEvent type-argument</typeparam>
    public abstract class BaseDomainEvent<TAggregate, TEvent> : IDomainEvent<TAggregate>
        where TAggregate : class, IAggregate<TAggregate>, IHasDomainEvent<TAggregate, TEvent>
        where TEvent : class, IDomainEvent<TAggregate>
    {
        /// <summary> .cctor </summary>
        /// <param name="aggregateId">Aggregate id</param>
        /// <param name="index">Index</param>
        /// <param name="timestamp">Timestamp</param>
        protected BaseDomainEvent(
            Guid aggregateId,
            long index,
            DateTime timestamp)
        {
            AggregateId = aggregateId;
            Index = index;
            Timestamp = timestamp;
        }

        /// <inheritdoc />
        public Guid AggregateId { get; private init; }

        /// <inheritdoc />
        public long Index { get; private init; }

        /// <inheritdoc />
        public DateTime Timestamp { get; private init; }

        /// <inheritdoc />
        public void Apply(TAggregate aggregate)
        {
            aggregate.Apply((this as TEvent) !);
        }
    }
}