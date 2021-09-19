namespace SpaceEngineers.Core.DataAccess.Api.Transaction
{
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IDatabaseTransaction
    /// </summary>
    public interface IDatabaseTransaction : IDatabaseContext
    {
        /// <summary>
        /// Opens database transaction
        /// </summary>
        /// <param name="commit">Commit transaction or not</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<IAsyncDisposable> Open(bool commit, CancellationToken token);

        /// <summary>
        /// Opens database transaction
        /// </summary>
        /// <param name="commit">Commit transaction or not</param>
        /// <param name="isolationLevel">Isolation level</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<IAsyncDisposable> Open(bool commit, IsolationLevel isolationLevel, CancellationToken token);
    }
}