namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql
{
    using System;
    using Api.Exceptions;
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
        public void Handle(string commandText, Exception exception)
        {
            var databaseException = exception.IsSerializationFailure()
                ? new DatabaseConcurrentUpdateException(commandText, exception)
                : new DatabaseException(commandText, exception);

            throw databaseException;
        }
    }
}