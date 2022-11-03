namespace SpaceEngineers.Core.DataAccess.Api.Exceptions
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
            : base($"Concurrent update exception: {commandText}", innerException)
        {
            CommandText = commandText;
        }

        /// <summary>
        /// Command text
        /// </summary>
        public string CommandText { get; }
    }
}