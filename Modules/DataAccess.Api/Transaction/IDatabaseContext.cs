namespace SpaceEngineers.Core.DataAccess.Api.Transaction
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
        Task Track(IAggregate aggregate, CancellationToken token);
    }
}