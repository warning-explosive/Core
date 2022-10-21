namespace SpaceEngineers.Core.DataAccess.Orm.Host.Abstractions
{
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// IDatabaseProvider
    /// </summary>
    public interface IDatabaseProvider
    {
        /// <summary>
        /// Gets implementation for following composition
        /// </summary>
        /// <returns>Gets implementation details</returns>
        IEnumerable<Assembly> Implementation();
    }
}