namespace SpaceEngineers.Core.DataAccess.Contract.Abstractions
{
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using GenericDomain.Api.Abstractions;

    /// <summary>
    /// IDatabaseTransaction
    /// </summary>
    public interface IDatabaseTransaction : IResolvable
    {
        /// <summary>
        /// Are there any changes in the database transaction
        /// </summary>
        bool HasChanges { get; }

        /// <summary>
        /// Opens database transaction
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<IDbTransaction> Open(CancellationToken token);

        /// <summary>
        /// Closes database transaction
        /// </summary>
        /// <param name="commit">Commit transaction or not</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Close(bool commit, CancellationToken token);

        /// <summary>
        /// Start tracking specified aggregate root
        /// </summary>
        /// <param name="aggregate">Aggregate root</param>
        /// <typeparam name="TAggregate">TAggregate type-argument</typeparam>
        void Track<TAggregate>(TAggregate aggregate)
            where TAggregate : class, IAggregate;
    }
}