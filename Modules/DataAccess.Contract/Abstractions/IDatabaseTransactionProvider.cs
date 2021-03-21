namespace SpaceEngineers.Core.DataAccess.Contract.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IDatabaseTransactionProvider
    /// </summary>
    public interface IDatabaseTransactionProvider : IResolvable
    {
        /// <summary>
        /// OpenTransaction
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing open operation</returns>
        Task OpenTransaction(CancellationToken token);

        /// <summary>
        /// Commit opened transaction
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing commit operation</returns>
        Task Commit(CancellationToken token);

        /// <summary>
        /// Rollback opened transaction
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing rollback operation</returns>
        Task Rollback(CancellationToken token);
    }
}