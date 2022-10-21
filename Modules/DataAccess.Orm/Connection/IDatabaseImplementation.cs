namespace SpaceEngineers.Core.DataAccess.Orm.Connection
{
    using System;

    /// <summary>
    /// IDatabaseImplementation
    /// </summary>
    public interface IDatabaseImplementation
    {
        /// <summary>
        /// Handles ORM exception and throws provider dependent exceptions
        /// </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="exception">Exception</param>
        void Handle(string commandText, Exception exception);
    }
}