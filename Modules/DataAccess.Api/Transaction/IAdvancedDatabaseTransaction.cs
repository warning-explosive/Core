namespace SpaceEngineers.Core.DataAccess.Api.Transaction
{
    using System.Collections.Generic;
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
        /// Gets collected changes
        /// </summary>
        IReadOnlyCollection<ITransactionalChange> Changes { get; }

        /// <summary>
        /// Collects change
        /// </summary>
        /// <param name="change">ITransactionalChange</param>
        void CollectChange(ITransactionalChange change);
    }
}