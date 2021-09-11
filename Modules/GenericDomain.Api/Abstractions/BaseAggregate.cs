namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System.Collections.Generic;

    /// <summary>
    /// BaseAggregate
    /// </summary>
    public abstract class BaseAggregate : BaseEntity, IAggregate
    {
        private readonly List<IDomainEvent> _events;

        /// <summary> .cctor </summary>
        protected BaseAggregate()
        {
            // TODO: #132 - generate BaseAggregateCreated event
            _events = new List<IDomainEvent>();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IDomainEvent> Events => _events;

        /// <summary>
        /// Populates domain event
        /// </summary>
        /// <param name="events">Domain events</param>
        protected void PopulateEvent(params IDomainEvent[] events)
        {
            // TODO: #132 - publish domain events during transaction committing
            _events.AddRange(events);
        }
    }
}