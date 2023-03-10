namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Transaction
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
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Apply(
            IAdvancedDatabaseTransaction transaction,
            CancellationToken token);

        /// <summary>
        /// Applies transactional change to transactional store
        /// </summary>
        /// <param name="transactionalStore">ITransactionalStore</param>
        void Apply(ITransactionalStore transactionalStore);
    }
}