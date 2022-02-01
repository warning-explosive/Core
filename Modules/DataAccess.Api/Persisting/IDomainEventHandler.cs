namespace SpaceEngineers.Core.DataAccess.Api.Persisting
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using GenericDomain.Api.Abstractions;

    /// <summary>
    /// IDomainEventHandler
    /// </summary>
    /// <typeparam name="TEvent">TEvent type-argument</typeparam>
    public interface IDomainEventHandler<in TEvent> : IResolvable
        where TEvent : IDomainEvent
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