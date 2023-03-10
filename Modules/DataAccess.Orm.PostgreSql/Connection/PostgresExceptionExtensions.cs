namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Connection
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using System.Threading.Tasks;
    using Basics;
    using Npgsql;
    using Sql.Exceptions;
    using Sql.Translation;

    internal static class PostgresExceptionExtensions
    {
        public static bool IsUniqueViolation(this Exception exception)
        {
            return exception is PostgresException postgresException
                && postgresException.SqlState.Equals(PostgresErrorCodes.UniqueViolation, StringComparison.OrdinalIgnoreCase);
        }

        public static bool DatabaseDoesNotExist(this Exception exception)
        {
            return exception is PostgresException postgresException
                && postgresException.SqlState.Equals(PostgresErrorCodes.InvalidCatalogName, StringComparison.OrdinalIgnoreCase);
        }

        public static async Task<T> Handled<T>(
            this Task<T> task,
            NpgsqlCommand command)
        {
            try
            {
                return await task.ConfigureAwait(false);
            }
            catch (PostgresException exception)
            {
                return Handle<T>(exception, command);
            }
        }

        public static async Task<T> Handled<T>(
            this Task<T> task,
            NpgsqlBatch batch)
        {
            try
            {
                return await task.ConfigureAwait(false);
            }
            catch (PostgresException exception)
            {
                return Handle<T>(exception, batch);
            }
        }

        private static TResult Handle<TResult>(Exception exception, NpgsqlCommand command)
        {
            var sqlCommand = new SqlCommand(
                command.CommandText,
                command.Parameters.Select(param => new SqlCommandParameter(param.ParameterName, param.Value, typeof(object))).ToList());

            DatabaseException databaseException = exception.Flatten().Any(ex => ex.IsSerializationFailure())
                ? new DatabaseConcurrentUpdateException(sqlCommand.ToString(), exception)
                : new DatabaseCommandExecutionException(sqlCommand.ToString(), exception);

            throw databaseException;
        }

        private static TResult Handle<TResult>(Exception exception, NpgsqlBatch batch)
        {
            var sqlCommand = batch
                .BatchCommands
                .AsEnumerable<DbBatchCommand>()
                .Select(command => new SqlCommand(
                    command.CommandText,
                    command.Parameters.AsEnumerable<NpgsqlParameter>().Select(param => new SqlCommandParameter(param.ParameterName, param.Value, typeof(object))).ToList()))
                    .Aggregate((prev, next) => prev.Merge(next, ";" + Environment.NewLine));

            DatabaseException databaseException = exception.Flatten().Any(ex => ex.IsSerializationFailure())
                ? new DatabaseConcurrentUpdateException(sqlCommand.ToString(), exception)
                : new DatabaseCommandExecutionException(sqlCommand.ToString(), exception);

            throw databaseException;
        }

        private static bool IsSerializationFailure(this Exception exception)
        {
            return exception is PostgresException postgresException
                   && postgresException.SqlState.Equals(PostgresErrorCodes.SerializationFailure, StringComparison.OrdinalIgnoreCase);
        }
    }
}