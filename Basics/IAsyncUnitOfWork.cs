namespace SpaceEngineers.Core.Basics
{
    using System;

    /// <summary>
    /// Represents logical transaction that tracks different kinds of resources and maintains consistency
    /// </summary>
    /// <typeparam name="TContext">TContext type-argument</typeparam>
    public interface IAsyncUnitOfWork<TContext> : IAsyncDisposable
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
    }
}