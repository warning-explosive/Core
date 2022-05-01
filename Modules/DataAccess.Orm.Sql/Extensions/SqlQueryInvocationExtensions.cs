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
        public static Task<int> InvokeScalar(
            this IAdvancedDatabaseTransaction transaction,
            string commandText,
            OrmSettings settings,
            CancellationToken token)
        {
            var command = new CommandDefinition(
                commandText,
                null,
                transaction.DbTransaction,
                settings.QueryTimeout.Seconds,
                CommandType.Text,
                CommandFlags.Buffered,
                token);

            return transaction
               .DbTransaction
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
            this IAdvancedDatabaseTransaction transaction,
            string commandText,
            OrmSettings settings,
            CancellationToken token)
        {
            var command = new CommandDefinition(
                commandText,
                null,
                transaction.DbTransaction,
                settings.QueryTimeout.Seconds,
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
        public static Task<int> InvokeScalar(
            this IDatabaseConnection connection,
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

            return connection
               .UnderlyingDbConnection
               .ExecuteAsync(command);
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
            var command = new CommandDefinition(
                commandText,
                null,
                null,
                settings.QueryTimeout.Seconds,
                CommandType.Text,
                CommandFlags.Buffered,
                token);

            return connection
               .UnderlyingDbConnection
               .QueryAsync(command);
        }
    }
}