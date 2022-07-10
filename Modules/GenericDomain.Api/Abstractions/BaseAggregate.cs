namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// BaseAggregate
    /// </summary>
    /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
    public abstract class BaseAggregate<TAggregate> : BaseEntity, IAggregate
        where TAggregate : class, IAggregate<TAggregate>
    {
        private readonly ICollection<IDomainEvent> _events;
        private long _index;

        /// <summary> .cctor </summary>
        /// <param name="events">Domain events</param>
        protected BaseAggregate(IEnumerable<IDomainEvent<TAggregate>> events)
        {
            _events = new List<IDomainEvent>();
            _index = -1;

            foreach (var domainEvent in events)
            {
                domainEvent.Apply((this as TAggregate) !);

                _index++;
            }
        }

        internal static event EventHandler<IDomainEvent>? OnDomainEvent;

        /// <inheritdoc />
        public IEnumerable<IDomainEvent> Events => _events;

        /// <summary>
        /// Gets next domain event index without any state changes
        /// </summary>
        /// <returns>Next domain event index</returns>
        protected long NextDomainEventIndex() => _index + 1;

        /// <summary>
        /// Populates domain event and increases domain event index counter
        /// </summary>
        /// <param name="domainEvent">Domain event</param>
        protected void PopulateEvent(IDomainEvent domainEvent)
        {
            _events.Add(domainEvent);

            _index++;

            OnDomainEvent?.Invoke(this, domainEvent);
        }
    }
}