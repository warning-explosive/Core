namespace SpaceEngineers.Core.DataAccess.Api.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using GenericDomain.Api.Abstractions;

    /// <summary>
    /// IDatabaseContext
    /// </summary>
    public interface IDatabaseContext : IResolvable
    {
        /// <summary>
        /// Are there any changes in the database transaction
        /// </summary>
        bool HasChanges { get; }

        /// <summary>
        /// Starts tracking specified aggregate root with insert semantics
        /// </summary>
        /// <param name="aggregate">Aggregate root</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Insert(IAggregate aggregate, CancellationToken token);

        /// <summary>
        /// Starts tracking specified aggregate root with upsert update semantics
        /// </summary>
        /// <param name="aggregate">Aggregate root</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Update(IAggregate aggregate, CancellationToken token);

        /// <summary>
        /// Starts tracking specified aggregate root with upsert (insert | update) semantics
        /// </summary>
        /// <param name="aggregate">Aggregate root</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Upsert(IAggregate aggregate, CancellationToken token);

        /// <summary>
        /// Starts tracking specified aggregate root with remove semantics
        /// </summary>
        /// <param name="aggregate">Aggregate root</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Delete(IAggregate aggregate, CancellationToken token);
    }
}