namespace SpaceEngineers.Core.DataAccess.Orm.Connection
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// IDatabaseProvider
    /// </summary>
    public interface IDatabaseProvider
    {
        /// <summary>
        /// Database provider implementation
        /// </summary>
        /// <returns>Implementation</returns>
        IEnumerable<Assembly> Implementation();

        /// <summary>
        /// Database provider implementation for migrations
        /// </summary>
        /// <returns>Implementation</returns>
        IEnumerable<Assembly> Migrations();

        /// <summary>
        /// Handles ORM exception and throws provider dependent exceptions
        /// </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="exception">Exception</param>
        void Handle(string commandText, Exception exception);
    }
}