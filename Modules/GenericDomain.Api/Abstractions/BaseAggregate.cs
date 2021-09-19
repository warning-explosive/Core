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
            _events = new List<IDomainEvent>();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IDomainEvent> Events => _events;

        /// <summary>
        /// Populates domain event
        /// </summary>
        /// <param name="domainEvent">Domain event</param>
        protected void PopulateEvent(IDomainEvent domainEvent)
        {
            _events.Add(domainEvent);
        }
    }
}