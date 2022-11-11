namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System.Collections.Generic;

    /// <summary>
    /// IAggregate
    /// </summary>
    public interface IAggregate : IEntity
    {
        /// <summary>
        /// Domain events
        /// </summary>
        IReadOnlyCollection<IDomainEvent> Events { get; }
    }

    /// <summary>
    /// IAggregate
    /// </summary>
    /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
    public interface IAggregate<TAggregate> : IAggregate
        where TAggregate : class, IAggregate<TAggregate>
    {
        /// <summary>
        /// Populates domain event and maintains internal state
        /// </summary>
        /// <param name="domainEvent">Domain event</param>
        /// <typeparam name="TEvent">TEvent type-argument</typeparam>
        void PopulateEvent<TEvent>(TEvent domainEvent)
            where TEvent : class, IDomainEvent<TAggregate>;
    }

    /// <summary>
    /// IHasDomainEvent
    /// </summary>
    /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
    /// <typeparam name="TEvent">TEvent type-argument</typeparam>
    public interface IHasDomainEvent<TAggregate, TEvent>
        where TAggregate : class, IAggregate<TAggregate>
        where TEvent : class, IDomainEvent<TAggregate>
    {
        /// <summary>
        /// Applies domain event state to current aggregate
        /// </summary>
        /// <param name="domainEvent">Domain event</param>
        void Apply(TEvent domainEvent);
    }
}