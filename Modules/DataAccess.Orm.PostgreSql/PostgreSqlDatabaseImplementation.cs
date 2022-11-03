namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql
{
    using System.Linq;
    using Api.Exceptions;
    using Basics;
    using Extensions;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Orm.Connection;

    [Component(EnLifestyle.Singleton)]
    internal class PostgreSqlDatabaseImplementation : IDatabaseImplementation,
                                                      IResolvable<IDatabaseImplementation>
    {
        /// <inheritdoc />
        public void Handle(DatabaseCommandExecutionException exception)
        {
            DatabaseException databaseException = exception.Flatten().Any(ex => ex.IsSerializationFailure())
                ? new DatabaseConcurrentUpdateException(exception.CommandText, exception)
                : exception;

            throw databaseException;
        }
    }
}