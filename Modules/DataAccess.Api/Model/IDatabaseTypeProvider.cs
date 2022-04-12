namespace SpaceEngineers.Core.DataAccess.Api.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// IDatabaseTypeProvider
    /// </summary>
    public interface IDatabaseTypeProvider
    {
        /// <summary>
        /// Types that represents database entities (tables)
        /// </summary>
        /// <returns>Database entities (tables)</returns>
        IEnumerable<Type> DatabaseEntities();
    }
}