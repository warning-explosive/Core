namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IDomainEventHandler
    /// </summary>
    /// <typeparam name="TEvent">TEvent type-argument</typeparam>
    public interface IDomainEventHandler<in TEvent>
        where TEvent : class, IDomainEvent
    {
        /// <summary>
        /// Handles domain event
        /// </summary>
        /// <param name="domainEvent">Domain event</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Handle(TEvent domainEvent, CancellationToken token);
    }
}