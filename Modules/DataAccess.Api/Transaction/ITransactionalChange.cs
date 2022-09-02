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
        /// Applies transactional change to physical storage
        /// </summary>
        /// <param name="databaseTransaction">IAdvancedDatabaseTransaction</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Apply(
            IAdvancedDatabaseTransaction databaseTransaction,
            CancellationToken token);

        /// <summary>
        /// Applies transactional change to transactional store
        /// </summary>
        /// <param name="transactionalStore">ITransactionalStore</param>
        void Apply(ITransactionalStore transactionalStore);
    }
}