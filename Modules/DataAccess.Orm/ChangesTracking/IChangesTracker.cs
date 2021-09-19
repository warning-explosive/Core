namespace SpaceEngineers.Core.DataAccess.Orm.ChangesTracking
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using GenericDomain.Api.Abstractions;

    /// <summary>
    /// IChangesTracker
    /// </summary>
    public interface IChangesTracker : IResolvable
    {
        /// <summary>
        /// Did the changes tracker capture any state changes
        /// </summary>
        bool HasChanges { get; }

        /// <summary>
        /// Starts track aggregate changes in the transaction
        /// </summary>
        /// <param name="aggregate">Aggregate root</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Track(IAggregate aggregate, CancellationToken token);

        /// <summary>
        /// Saves change in database within opened transaction
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task SaveChanges(CancellationToken token);
    }
}