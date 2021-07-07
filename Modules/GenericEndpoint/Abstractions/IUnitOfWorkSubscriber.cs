namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IUnitOfWorkSubscriber
    /// </summary>
    /// <typeparam name="TContext">TContext type-argument</typeparam>
    public interface IUnitOfWorkSubscriber<in TContext>
    {
        /// <summary>
        /// Fires when logical transaction is starting
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing start operation</returns>
        Task OnStart(TContext context, CancellationToken token);

        /// <summary>
        /// Fires when logical transaction is committing
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing commit operation</returns>
        Task OnCommit(TContext context, CancellationToken token);

        /// <summary>
        /// Fires when logical transaction is rolling back
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing rollback operation</returns>
        Task OnRollback(TContext context, CancellationToken token);
    }
}