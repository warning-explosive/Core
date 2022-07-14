namespace SpaceEngineers.Core.DataAccess.Api.Transaction
{
    using System.Data;

    /// <summary>
    /// IAdvancedDatabaseTransaction
    /// </summary>
    public interface IAdvancedDatabaseTransaction : IDatabaseTransactionStore
    {
        /// <summary>
        /// Gets underlying db transaction and begins it if necessary
        /// </summary>
        IDbTransaction DbTransaction { get; }

        /// <summary>
        /// Gets underlying db connection and connects if necessary
        /// </summary>
        IDatabaseConnection DbConnection { get; }

        /// <summary>
        /// Returns true if connection to the database was requested earlier
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Collects change
        /// </summary>
        /// <param name="change">ITransactionalChange</param>
        void CollectChange(ITransactionalChange change);
    }
}