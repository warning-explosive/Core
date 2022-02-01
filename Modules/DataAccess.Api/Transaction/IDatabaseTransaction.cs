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
        /// Gets underlying db transaction and begins it if necessary
        /// </summary>
        IDbTransaction UnderlyingDbTransaction { get; }

        /// <summary>
        /// Opens database transaction scope
        /// </summary>
        /// <param name="commit">Commit transaction or not</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<IAsyncDisposable> OpenScope(bool commit, CancellationToken token);

        /// <summary>
        /// Opens database transaction
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Open(CancellationToken token);

        /// <summary>
        /// Closes database transaction
        /// </summary>
        /// <param name="commit">Commit transaction or not</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Close(bool commit, CancellationToken token);
    }
}