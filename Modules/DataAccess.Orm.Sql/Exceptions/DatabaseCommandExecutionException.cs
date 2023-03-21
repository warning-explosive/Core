namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Exceptions
{
    using System;

    /// <summary>
    /// DatabaseCommandExecutionException
    /// </summary>
    public class DatabaseCommandExecutionException : DatabaseException
    {
        /// <summary> .cctor </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="innerException">Inner exception</param>
        public DatabaseCommandExecutionException(string commandText, Exception innerException)
            : base($"Command execution error: {commandText}", innerException)
        {
            CommandText = commandText;
        }

        /// <summary>
        /// Command text
        /// </summary>
        public string CommandText { get; }
    }
}