namespace SpaceEngineers.Core.DataAccess.Api.Persisting
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using GenericDomain.Api.Abstractions;

    /// <summary>
    /// IDatabaseStateTransformer
    /// </summary>
    /// <typeparam name="TEvent">TEvent type-argument</typeparam>
    public interface IDatabaseStateTransformer<in TEvent> : IResolvable
        where TEvent : IDomainEvent
    {
        /// <summary>
        /// Persists domain event into database
        /// </summary>
        /// <param name="domainEvent">Domain event</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Persist(TEvent domainEvent, CancellationToken token);
    }
}