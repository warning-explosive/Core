namespace SpaceEngineers.Core.DataAccess.Api.Abstractions
{
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IDatabaseTransaction
    /// </summary>
    public interface IDatabaseTransaction : IDatabaseContext
    {
        /// <summary>
        /// Underlying db transaction
        /// </summary>
        IDbTransaction UnderlyingDbTransaction { get; }

        /// <summary>
        /// Opens database transaction
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<IDbTransaction> Open(CancellationToken token);

        /// <summary>
        /// Opens database transaction
        /// </summary>
        /// <param name="isolationLevel">Isolation level</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<IDbTransaction> Open(IsolationLevel isolationLevel, CancellationToken token);

        /// <summary>
        /// Closes database transaction
        /// </summary>
        /// <param name="commit">Commit transaction or not</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Close(bool commit, CancellationToken token);
    }
}