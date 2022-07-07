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

        /// <summary> .cctor </summary>
        /// <param name="events">Domain events</param>
        protected BaseAggregate(IEnumerable<IDomainEvent<TAggregate>> events)
        {
            _events = new List<IDomainEvent>();

            foreach (var domainEvent in events)
            {
                domainEvent.Apply((this as TAggregate) !);
            }
        }

        internal static event EventHandler<IDomainEvent>? OnDomainEvent;

        /// <inheritdoc />
        public IEnumerable<IDomainEvent> Events => _events;

        /// <summary>
        /// Populates domain event
        /// </summary>
        /// <param name="domainEvent">Domain event</param>
        protected void PopulateEvent(IDomainEvent domainEvent)
        {
            _events.Add(domainEvent);

            OnDomainEvent?.Invoke(this, domainEvent);
        }
    }
}