namespace SpaceEngineers.Core.GenericDomain.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;

    /// <summary>
    /// IEventStore
    /// </summary>
    [SuppressMessage("Analysis", "CA1716", Justification = "desired name")]
    public interface IEventStore
    {
        /// <summary>
        /// Gets latest aggregate by it's identifier
        /// </summary>
        /// <param name="aggregateId">Aggregate identifier</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        Task<TAggregate?> GetAggregate<TAggregate>(
            Guid aggregateId,
            CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>;

        /// <summary>
        /// Gets aggregate by it's identifier and version
        /// </summary>
        /// <param name="aggregateId">Aggregate identifier</param>
        /// <param name="version">Aggregate's version</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        Task<TAggregate?> GetAggregate<TAggregate>(
            Guid aggregateId,
            long version,
            CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>;

        /// <summary>
        /// Gets aggregate by it's identifier and version timestamp
        /// </summary>
        /// <param name="aggregateId">Aggregate identifier</param>
        /// <param name="timestamp">Aggregate's version timestamp</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        Task<TAggregate?> GetAggregate<TAggregate>(
            Guid aggregateId,
            DateTime timestamp,
            CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>;

        /// <summary>
        /// Appends event to aggregate's event store
        /// </summary>
        /// <param name="args">DomainEventArgs</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Append(
            DomainEventArgs args,
            CancellationToken token);

        /// <summary>
        /// Appends domain events to aggregate's event store
        /// </summary>
        /// <param name="args">DomainEventArgs</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Append(
            IReadOnlyCollection<DomainEventArgs> args,
            CancellationToken token);
    }
}