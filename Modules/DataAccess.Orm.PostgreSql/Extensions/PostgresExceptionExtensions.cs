namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Extensions
{
    using System;
    using Npgsql;

    internal static class PostgresExceptionExtensions
    {
        internal static bool IsSerializationFailure(this Exception exception)
        {
            return exception is PostgresException postgresException
                && postgresException.SqlState.Equals(PostgresErrorCodes.SerializationFailure, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsUniqueViolation(this Exception exception)
        {
            return exception is PostgresException postgresException
                && postgresException.SqlState.Equals(PostgresErrorCodes.UniqueViolation, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool DatabaseDoesNotExist(this Exception exception)
        {
            return exception is PostgresException postgresException
                && postgresException.SqlState.Equals(PostgresErrorCodes.InvalidCatalogName, StringComparison.OrdinalIgnoreCase);
        }
    }
}