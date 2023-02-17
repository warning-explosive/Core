namespace SpaceEngineers.Core.DataAccess.Orm.Connection
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Linq;
    using Transaction;

    /// <summary>
    /// IDatabaseConnectionProvider
    /// </summary>
    public interface IDatabaseConnectionProvider
    {
        /// <summary>
        /// Host
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Database
        /// </summary>
        string Database { get; }

        /// <summary>
        /// Isolation level
        /// </summary>
        IsolationLevel IsolationLevel { get; }

        /// <summary>
        /// Checks the database existence
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<bool> DoesDatabaseExist(CancellationToken token);

        /// <summary>
        /// Opens DB connection
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<IDbConnection> OpenConnection(CancellationToken token);

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
        /// <returns>Rows</returns>
        IAsyncEnumerable<IDictionary<string, object?>> Query(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            CancellationToken token);

        /// <summary>
        /// Executes database command
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="command">Command</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Rows</returns>
        IAsyncEnumerable<IDictionary<string, object?>> Query(
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