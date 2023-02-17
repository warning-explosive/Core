namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Connection
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Api.Exceptions;
    using Basics;
    using Linq;
    using Npgsql;

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
            ICommand command)
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

        private static TResult Handle<TResult>(Exception exception, ICommand command)
        {
            DatabaseException databaseException = exception.Flatten().Any(ex => ex.IsSerializationFailure())
                ? new DatabaseConcurrentUpdateException(command.ToString(), exception)
                : new DatabaseCommandExecutionException(command.ToString(), exception);

            throw databaseException;
        }

        private static bool IsSerializationFailure(this Exception exception)
        {
            return exception is PostgresException postgresException
                   && postgresException.SqlState.Equals(PostgresErrorCodes.SerializationFailure, StringComparison.OrdinalIgnoreCase);
        }
    }
}