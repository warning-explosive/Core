﻿namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System.Collections.Generic;
    using System.Data;
    using Api.Transaction;
    using Linq;

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
        IDbConnection DbConnection { get; }

        /// <summary>
        /// Are there any changes in the database transaction
        /// </summary>
        bool HasChanges { get; }

        /// <summary>
        /// Returns commands that were executed previously
        /// </summary>
        IReadOnlyCollection<ICommand> Commands { get; }

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

        /// <summary>
        /// Collects command
        /// </summary>
        /// <param name="command">ICommand</param>
        void CollectCommand(ICommand command);
    }
}