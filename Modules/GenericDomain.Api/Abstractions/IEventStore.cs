namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IEventStore
    /// </summary>
    [SuppressMessage("Analysis", "CA1716", Justification = "desired name")]
    public interface IEventStore
    {
        /// <summary>
        /// Gets aggregate by it's identifier and version timestamp
        /// </summary>
        /// <param name="aggregateId">Aggregate identifier</param>
        /// <param name="timestamp">Timestamp</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
        /// <returns>Aggregate</returns>
        Task<TAggregate?> Get<TAggregate>(
            Guid aggregateId,
            DateTime timestamp,
            CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>;

        /// <summary>
        /// Appends event to aggregate's event store
        /// </summary>
        /// <param name="domainEvent">Domain event</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
        /// <typeparam name="TEvent">TEvent type-argument</typeparam>
        /// <returns>Aggregate</returns>
        Task Append<TAggregate, TEvent>(
            TEvent domainEvent,
            CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>
            where TEvent : class, IDomainEvent;
    }
}