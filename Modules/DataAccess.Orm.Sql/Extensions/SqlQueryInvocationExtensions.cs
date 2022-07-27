namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Extensions
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Transaction;
    using Basics;
    using Dapper;
    using Microsoft.Extensions.Logging;
    using Orm.Settings;

    /// <summary>
    /// SQL query invocation extensions
    /// </summary>
    public static class SqlQueryInvocationExtensions
    {
        /// <summary>
        /// Invokes query
        /// </summary>
        /// <param name="transaction">IDbTransaction</param>
        /// <param name="commandText">Sql command text</param>
        /// <param name="settings">Orm settings</param>
        /// <param name="logger">ILogger</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Query result</returns>
        public static Task<IEnumerable<dynamic>> Query(
            this IAdvancedDatabaseTransaction transaction,
            string commandText,
            OrmSettings settings,
            ILogger logger,
            CancellationToken token)
        {
            return transaction.Query((commandText, settings, logger), token);
        }

        /// <summary>
        /// Invokes query
        /// </summary>
        /// <param name="transaction">IDbTransaction</param>
        /// <param name="state">State</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Query result</returns>
        public static Task<IEnumerable<dynamic>> Query(
            this IAdvancedDatabaseTransaction transaction,
            (string commandText, OrmSettings settings, ILogger logger) state,
            CancellationToken token)
        {
            if (state.settings.DumpQueries)
            {
                state.logger.Debug(state.commandText);
            }

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
        /// Invokes query
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="commandText">Sql command text</param>
        /// <param name="settings">Orm settings</param>
        /// <param name="logger">ILogger</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Query result</returns>
        public static Task<IEnumerable<dynamic>> Query(
            this IDatabaseConnection connection,
            string commandText,
            OrmSettings settings,
            ILogger logger,
            CancellationToken token)
        {
            return connection.Query((commandText, settings, logger), token);
        }

        /// <summary>
        /// Invokes query
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="state">State</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Query result</returns>
        public static Task<IEnumerable<dynamic>> Query(
            this IDatabaseConnection connection,
            (string commandText, OrmSettings settings, ILogger logger) state,
            CancellationToken token)
        {
            if (state.settings.DumpQueries)
            {
                state.logger.Debug(state.commandText);
            }

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

        /// <summary>
        /// Executes command
        /// </summary>
        /// <param name="transaction">IDbTransaction</param>
        /// <param name="commandText">Sql command text</param>
        /// <param name="settings">Orm settings</param>
        /// <param name="logger">ILogger</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The number of rows affected</returns>
        public static Task<long> Execute(
            this IAdvancedDatabaseTransaction transaction,
            string commandText,
            OrmSettings settings,
            ILogger logger,
            CancellationToken token)
        {
            return transaction.Execute((commandText, settings, logger), token);
        }

        /// <summary>
        /// Executes command
        /// </summary>
        /// <param name="transaction">IDbTransaction</param>
        /// <param name="state">State</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The number of rows affected</returns>
        public static async Task<long> Execute(
            this IAdvancedDatabaseTransaction transaction,
            (string commandText, OrmSettings settings, ILogger logger) state,
            CancellationToken token)
        {
            if (state.settings.DumpQueries)
            {
                state.logger.Debug(state.commandText);
            }

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
        /// Invokes scalar command
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="commandText">Sql command text</param>
        /// <param name="settings">Orm settings</param>
        /// <param name="logger">ILogger</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The number of rows affected</returns>
        public static Task<long> Execute(
            this IDatabaseConnection connection,
            string commandText,
            OrmSettings settings,
            ILogger logger,
            CancellationToken token)
        {
            return connection.Execute((commandText, settings, logger), token);
        }

        /// <summary>
        /// Executes command
        /// </summary>
        /// <param name="connection">IDbConnection</param>
        /// <param name="state">State</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The number of rows affected</returns>
        public static async Task<long> Execute(
            this IDatabaseConnection connection,
            (string commandText, OrmSettings settings, ILogger logger) state,
            CancellationToken token)
        {
            if (state.settings.DumpQueries)
            {
                state.logger.Debug(state.commandText);
            }

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
    }
}