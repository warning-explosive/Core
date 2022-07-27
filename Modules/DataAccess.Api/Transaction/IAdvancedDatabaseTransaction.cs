namespace SpaceEngineers.Core.DataAccess.Api.Transaction
{
    using System.Data;

    /// <summary>
    /// IAdvancedDatabaseTransaction
    /// </summary>
    public interface IAdvancedDatabaseTransaction : IDatabaseTransaction
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
        /// Gets access to transactional store
        /// </summary>
        ITransactionalStore Store { get; }

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