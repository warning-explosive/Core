namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System;
    using System.Collections.Generic;
    using Basics;

    /// <summary>
    /// BaseAggregate
    /// </summary>
    /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
    public abstract class BaseAggregate<TAggregate> : BaseEntity, IAggregate<TAggregate>
        where TAggregate : class, IAggregate<TAggregate>
    {
        private readonly List<IDomainEvent> _events;
        private long _index;

        /// <summary> .cctor </summary>
        /// <param name="events">Domain events</param>
        protected BaseAggregate(IDomainEvent<TAggregate>[] events)
        {
            _events = new List<IDomainEvent>();
            _index = -1;

            foreach (var domainEvent in events)
            {
                Apply(domainEvent);

                _index++;
            }
        }

        /// <summary>
        /// OnDomainEvent
        /// </summary>
        public static event EventHandler<DomainEventArgs>? OnDomainEvent;

        /// <inheritdoc />
        public IReadOnlyCollection<IDomainEvent> Events => _events;

        /// <summary>
        /// Populates domain event and maintains internal state
        /// </summary>
        /// <param name="domainEvent">Domain event</param>
        /// <typeparam name="TEvent">TEvent type-argument</typeparam>
        public void PopulateEvent<TEvent>(TEvent domainEvent)
            where TEvent : class, IDomainEvent<TAggregate>
        {
            _events.Add(domainEvent);

            var details = new DomainEventDetails(Id, _index + 1, DateTime.UtcNow);

            var args = new DomainEventArgs(details, domainEvent);

            OnDomainEvent?.Invoke(this, args);

            _index++;
        }

        private void Apply(IDomainEvent domainEvent)
        {
            _ = this
               .CallMethod(nameof(Apply)) // IHasDomainEvent.Apply
               .WithArguments(domainEvent)
               .Invoke();
        }
    }
}