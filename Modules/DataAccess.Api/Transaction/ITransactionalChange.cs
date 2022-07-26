namespace SpaceEngineers.Core.DataAccess.Api.Transaction
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// ITransactionalChange
    /// </summary>
    public interface ITransactionalChange
    {
        /// <summary>
        /// Applies transactional change
        /// </summary>
        /// <param name="databaseTransaction">IAdvancedDatabaseTransaction</param>
        /// <param name="logger">ILogger</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Apply(
            IAdvancedDatabaseTransaction databaseTransaction,
            ILogger logger,
            CancellationToken token);
    }
}