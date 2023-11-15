namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Exceptions
{
    using System;

    /// <summary>
    /// DatabaseException
    /// </summary>
    public abstract class DatabaseException : Exception
    {
        /// <summary> .cctor </summary>
        /// <param name="message">Message</param>
        protected DatabaseException(string message)
            : base(message)
        {
        }

        /// <summary> .cctor </summary>
        /// <param name="message">Message</param>
        /// <param name="innerException">Inner exception</param>
        protected DatabaseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}