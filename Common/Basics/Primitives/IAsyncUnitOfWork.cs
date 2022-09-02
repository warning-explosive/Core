namespace SpaceEngineers.Core.Basics.Primitives
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
        /// Executes producer within logical transaction
        /// </summary>
        /// <param name="context">Context data container</param>
        /// <param name="producer">Producer</param>
        /// <param name="saveChanges">Save changes. By default changes are going to be rolled back</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing start operation</returns>
        Task ExecuteInTransaction(
            TContext context,
            Func<TContext, CancellationToken, Task> producer,
            bool saveChanges,
            CancellationToken token);
    }
}