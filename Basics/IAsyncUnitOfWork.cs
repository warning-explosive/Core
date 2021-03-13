namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents logical transaction that tracks different kinds of resources and maintains consistency
    /// </summary>
    /// <typeparam name="TContext">TContext type-argument</typeparam>
    public interface IAsyncUnitOfWork<TContext>
    {
        /// <summary>
        /// Context data container
        /// </summary>
        TContext Context { get; }

        /// <summary>
        /// Save changes
        /// By default changes are going to be rolled back
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Starts logical transaction
        /// </summary>
        /// <param name="context">Context data container</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing start operation</returns>
        Task<IAsyncDisposable> StartTransaction(TContext context, CancellationToken token);
    }
}