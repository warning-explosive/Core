namespace SpaceEngineers.Core.GenericDomain.EventSourcing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;

    /// <summary>
    /// IDomainEventHandler
    /// </summary>
    /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
    /// <typeparam name="TEvent">TEvent type-argument</typeparam>
    public interface IDomainEventHandler<TAggregate, TEvent>
        where TAggregate : class, IAggregate<TAggregate>
        where TEvent : class, IDomainEvent<TAggregate>
    {
        /// <summary>
        /// Handles domain event
        /// </summary>
        /// <param name="domainEvent">Domain event</param>
        /// <param name="details">DomainEventDetails</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Handle(TEvent domainEvent, DomainEventDetails details, CancellationToken token);
    }
}