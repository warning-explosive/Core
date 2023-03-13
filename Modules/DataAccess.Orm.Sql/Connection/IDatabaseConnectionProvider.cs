namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Connection
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
    using Transaction;

    /// <summary>
    /// IDatabaseConnectionProvider
    /// </summary>
    public interface IDatabaseConnectionProvider
    {
        /// <summary>
        /// Checks the database existence
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<bool> DoesDatabaseExist(CancellationToken token);

        /// <summary>
        /// Gets transactional version for optimistic concurrency support
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<long> GetVersion(IAdvancedDatabaseTransaction transaction, CancellationToken token);

        /// <summary>
        /// Opens connection
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        ValueTask<IDbConnection> OpenConnection(CancellationToken token);

        /// <summary>
        /// Opens transaction
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>IDbTransaction</returns>
        ValueTask<IDbTransaction> BeginTransaction(IDbConnection connection, CancellationToken token);

        /// <summary>
        /// Executes database command
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="command">Command</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Number of affected rows</returns>
        Task<long> Execute(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            CancellationToken token);

        /// <summary>
        /// Executes database command
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="commands">Commands</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Number of affected rows</returns>
        Task<long> Execute(
            IAdvancedDatabaseTransaction transaction,
            IEnumerable<ICommand> commands,
            CancellationToken token);

        /// <summary>
        /// Executes database command
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="command">Command</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Number of affected rows</returns>
        Task<long> Execute(
            IDbConnection connection,
            ICommand command,
            CancellationToken token);

        /// <summary>
        /// Executes database command
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="commands">Commands</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Number of affected rows</returns>
        Task<long> Execute(
            IDbConnection connection,
            IEnumerable<ICommand> commands,
            CancellationToken token);

        /// <summary>
        /// Executes database command
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="command">Command</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Rows</returns>
        IAsyncEnumerable<IDictionary<string, object?>> Query<T>(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            CancellationToken token);

        /// <summary>
        /// Executes database command
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="command">Command</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Rows</returns>
        IAsyncEnumerable<IDictionary<string, object?>> Query<T>(
            IDbConnection connection,
            ICommand command,
            CancellationToken token);

        /// <summary>
        /// Executes scalar
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="command">Command</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Number of affected rows</returns>
        Task<T> ExecuteScalar<T>(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            CancellationToken token);

        /// <summary>
        /// Executes scalar
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="command">Command</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Number of affected rows</returns>
        Task<T> ExecuteScalar<T>(
            IDbConnection connection,
            ICommand command,
            CancellationToken token);
    }
}