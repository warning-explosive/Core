namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Extensions
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
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
        public static Task<int> InvokeScalar(
            this IDbTransaction transaction,
            string commandText,
            OrmSettings settings,
            CancellationToken token)
        {
            var command = new CommandDefinition(
                commandText,
                null,
                transaction,
                settings.QueryTimeout.Seconds,
                CommandType.Text,
                CommandFlags.Buffered,
                token);

            return transaction
                .Connection
                .ExecuteAsync(command);
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
            this IDbTransaction transaction,
            string commandText,
            OrmSettings settings,
            CancellationToken token)
        {
            var command = new CommandDefinition(
                commandText,
                null,
                transaction,
                settings.QueryTimeout.Seconds,
                CommandType.Text,
                CommandFlags.Buffered,
                token);

            return transaction
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
        public static Task<int> InvokeScalar(
            this IDbConnection connection,
            string commandText,
            OrmSettings settings,
            CancellationToken token)
        {
            var command = new CommandDefinition(
                commandText,
                null,
                null,
                settings.QueryTimeout.Seconds,
                CommandType.Text,
                CommandFlags.Buffered,
                token);

            return connection.ExecuteAsync(command);
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
            this IDbConnection connection,
            string commandText,
            OrmSettings settings,
            CancellationToken token)
        {
            var command = new CommandDefinition(
                commandText,
                null,
                null,
                settings.QueryTimeout.Seconds,
                CommandType.Text,
                CommandFlags.Buffered,
                token);

            return connection.QueryAsync(command);
        }
    }
}