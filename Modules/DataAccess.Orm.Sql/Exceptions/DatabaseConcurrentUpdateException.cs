namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Exceptions
{
    using System;

    /// <summary>
    /// DatabaseConcurrentUpdateException
    /// </summary>
    public sealed class DatabaseConcurrentUpdateException : DatabaseException
    {
        /// <summary> .cctor </summary>
        /// <param name="entity">Entity type</param>
        public DatabaseConcurrentUpdateException(Type entity)
            : base($"Entity {entity} have been tried to update concurrently")
        {
        }

        /// <summary> .cctor </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="innerException">Inner exception</param>
        public DatabaseConcurrentUpdateException(string commandText, Exception innerException)
            : base($"Concurrent update exception: {commandText}", innerException)
        {
        }
    }
}