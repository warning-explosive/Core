namespace SpaceEngineers.Core.DataAccess.Api.Exceptions
{
    using System;

    /// <summary>
    /// DatabaseException
    /// </summary>
    public class DatabaseException : Exception
    {
        /// <summary> .cctor </summary>
        /// <param name="message">Message</param>
        public DatabaseException(string message)
            : base(message)
        {
        }

        /// <summary> .cctor </summary>
        /// <param name="message">Message</param>
        /// <param name="innerException">Inner exception</param>
        public DatabaseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}