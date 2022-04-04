namespace SpaceEngineers.Core.DataAccess.Orm.Connection
{
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
    }
}