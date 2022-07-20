namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Extensions
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Transaction;
    using Dapper;
    using Orm.Settings;

    /// <summary>
    /// SQL query invocation extensions
    /// </summary>
    public static class SqlQueryInvocationExtensions
    {
        /// <summary>
        /// Invokes scalar command
        /// </summary>
        /// <param name="transaction">IDbTransaction</param>
        /// <param name="commandText">Sql command text</param>
        /// <param name="settings">Orm settings</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The number of rows affected</returns>
        public static Task<long> InvokeScalar(
            this IAdvancedDatabaseTransaction transaction,
            string commandText,
            OrmSettings settings,
            CancellationToken token)
        {
            return transaction.InvokeScalar((commandText, settings), token);
        }

        /// <summary>
        /// Invokes scalar command
        /// </summary>
        /// <param name="transaction">IDbTransaction</param>
        /// <param name="state">State</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The number of rows affected</returns>
        public static async Task<long> InvokeScalar(
            this IAdvancedDatabaseTransaction transaction,
            (string commandText, OrmSettings settings) state,
            CancellationToken token)
        {
            var command = new CommandDefinition(
                state.commandText,
                null,
                transaction.DbTransaction,
                state.settings.QueryTimeout.Seconds,
                CommandType.Text,
                CommandFlags.Buffered,
                token);

            return await transaction
               .DbTransaction
               .Connection
               .ExecuteAsync(command)
               .ConfigureAwait(false);
        }

        /// <summary>
        /// Invokes command
        /// </summary>
        /// <param name="transaction">IDbTransaction</param>
        /// <param name="commandText">Sql command text</param>
        /// <param name="settings">Orm settings</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Query result</returns>
        public static Task<IEnumerable<dynamic>> Invoke(
            this IAdvancedDatabaseTransaction transaction,
            string commandText,
            OrmSettings settings,
            CancellationToken token)
        {
            return transaction.Invoke((commandText, settings), token);
        }

        /// <summary>
        /// Invokes command
        /// </summary>
        /// <param name="transaction">IDbTransaction</param>
        /// <param name="state">State</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Query result</returns>
        public static Task<IEnumerable<dynamic>> Invoke(
            this IAdvancedDatabaseTransaction transaction,
            (string commandText, OrmSettings settings) state,
            CancellationToken token)
        {
            var command = new CommandDefinition(
                state.commandText,
                null,
                transaction.DbTransaction,
                state.settings.QueryTimeout.Seconds,
                CommandType.Text,
                CommandFlags.Buffered,
                token);

            return transaction
               .DbTransaction
               .Connection
               .QueryAsync(command);
        }

        /// <summary>
        /// Invokes scalar command
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="commandText">Sql command text</param>
        /// <param name="settings">Orm settings</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The number of rows affected</returns>
        public static Task<long> InvokeScalar(
            this IDatabaseConnection connection,
            string commandText,
            OrmSettings settings,
            CancellationToken token)
        {
            return connection.InvokeScalar((commandText, settings), token);
        }

        /// <summary>
        /// Invokes scalar command
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="state">State</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The number of rows affected</returns>
        public static async Task<long> InvokeScalar(
            this IDatabaseConnection connection,
            (string commandText, OrmSettings settings) state,
            CancellationToken token)
        {
            var command = new CommandDefinition(
                state.commandText,
                null,
                null,
                state.settings.QueryTimeout.Seconds,
                CommandType.Text,
                CommandFlags.Buffered,
                token);

            return await connection
               .UnderlyingDbConnection
               .ExecuteAsync(command)
               .ConfigureAwait(false);
        }

        /// <summary>
        /// Invokes command
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="commandText">Sql command text</param>
        /// <param name="settings">Orm settings</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Query result</returns>
        public static Task<IEnumerable<dynamic>> Invoke(
            this IDatabaseConnection connection,
            string commandText,
            OrmSettings settings,
            CancellationToken token)
        {
            return connection.Invoke((commandText, settings), token);
        }

        /// <summary>
        /// Invokes command
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="state">State</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Query result</returns>
        public static Task<IEnumerable<dynamic>> Invoke(
            this IDatabaseConnection connection,
            (string commandText, OrmSettings settings) state,
            CancellationToken token)
        {
            var command = new CommandDefinition(
                state.commandText,
                null,
                null,
                state.settings.QueryTimeout.Seconds,
                CommandType.Text,
                CommandFlags.Buffered,
                token);

            return connection
               .UnderlyingDbConnection
               .QueryAsync(command);
        }
    }
}