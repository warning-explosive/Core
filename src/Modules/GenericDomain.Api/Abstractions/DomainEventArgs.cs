namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Basics;
    using Basics.Exceptions;

    /// <summary>
    /// DomainEventArgs
    /// </summary>
    /// <typeparam name="TEvent">TEvent type-argument</typeparam>
    [SuppressMessage("Analysis", "SA1402", Justification = "generic data type with the same name")]
    public class DomainEventArgs<TEvent> : DomainEventArgs
    {
        /// <summary> .cctor </summary>
        /// <param name="args">DomainEventArgs</param>
        public DomainEventArgs(DomainEventArgs args)
            : base(args.AggregateId, args.DomainEvent, args.Index, args.Timestamp)
        {
            if (!args.DomainEvent.IsInstanceOfType(typeof(TEvent)))
            {
                throw new TypeMismatchException(typeof(TEvent), args.DomainEvent.GetType());
            }
        }

        /// <summary>
        /// Domain event
        /// </summary>
        public new TEvent DomainEvent => (TEvent)base.DomainEvent;
    }

    /// <summary>
    /// DomainEventArgs
    /// </summary>
    public class DomainEventArgs : EventArgs
    {
        /// <summary> .cctor </summary>
        /// <param name="aggregateId">AggregateId</param>
        /// <param name="domainEvent">IDomainEvent</param>
        /// <param name="index">Index</param>
        /// <param name="timestamp">Timestamp</param>
        public DomainEventArgs(
            Guid aggregateId,
            IDomainEvent domainEvent,
            long index,
            DateTime timestamp)
        {
            AggregateId = aggregateId;
            DomainEvent = domainEvent;
            Index = index;
            Timestamp = timestamp;
        }

        /// <summary>
        /// AggregateId
        /// </summary>
        public Guid AggregateId { get; init; }

        /// <summary>
        /// Domain event
        /// </summary>
        public IDomainEvent DomainEvent { get; init; }

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