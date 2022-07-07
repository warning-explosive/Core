namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
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
        /// <param name="id">Identifier</param>
        /// <param name="utcNow">Timestamp</param>
        /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
        /// <returns>Aggregate</returns>
        Task<TAggregate?> Get<TAggregate>(Guid id, DateTime utcNow)
            where TAggregate : class, IAggregate<TAggregate>;
    }
}