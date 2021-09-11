namespace SpaceEngineers.Core.DataAccess.Orm
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
    }
}