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
        /// Are there any changes in the database transaction
        /// </summary>
        bool HasChanges { get; }

        /// <summary>
        /// Gets access to transactional store
        /// </summary>
        ITransactionalStore Store { get; }

        /// <summary>
        /// Last successfully executed command
        /// </summary>
        string? LastCommand { get; }

        /// <summary>
        /// Returns true if connection to the database was requested earlier
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Collects change
        /// </summary>
        /// <param name="change">ITransactionalChange</param>
        void CollectChange(ITransactionalChange change);

        /// <summary>
        /// Collects command text
        /// </summary>
        /// <param name="commandText">Command text</param>
        void CollectCommand(string commandText);
    }
}