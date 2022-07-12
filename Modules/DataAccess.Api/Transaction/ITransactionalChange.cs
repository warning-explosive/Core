namespace SpaceEngineers.Core.DataAccess.Api.Transaction
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// ITransactionalChange
    /// </summary>
    public interface ITransactionalChange
    {
        /// <summary>
        /// Applies change
        /// </summary>
        /// <param name="databaseContext">IDatabaseContext</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Apply(
            IDatabaseContext databaseContext,
            CancellationToken token);
    }
}