namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Extensions
{
    using System;
    using Npgsql;

    /// <summary>
    /// PostgresExceptionExtensions
    /// </summary>
    public static class PostgresExceptionExtensions
    {
        /// <summary>
        /// IsSerializationFailure
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <returns>Result of check</returns>
        public static bool IsSerializationFailure(this Exception exception)
        {
            return exception is PostgresException postgresException
                && postgresException.SqlState.Equals(PostgresErrorCodes.SerializationFailure, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// IsUniqueViolation
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <returns>Result of check</returns>
        public static bool IsUniqueViolation(this Exception exception)
        {
            return exception is PostgresException postgresException
                && postgresException.SqlState.Equals(PostgresErrorCodes.UniqueViolation, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// DatabaseDoesNotExist
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <returns>Result of check</returns>
        public static bool DatabaseDoesNotExist(this Exception exception)
        {
            return exception is PostgresException postgresException
                && postgresException.SqlState.Equals(PostgresErrorCodes.InvalidCatalogName, StringComparison.OrdinalIgnoreCase);
        }
    }
}