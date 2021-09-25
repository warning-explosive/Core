namespace SpaceEngineers.Core.DataAccess.Api.Model
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IDatabaseTypeProvider
    /// </summary>
    public interface IDatabaseTypeProvider : IResolvable
    {
        /// <summary>
        /// Types that represents database entities (tables)
        /// </summary>
        /// <returns>Database entities (tables)</returns>
        IEnumerable<Type> DatabaseEntities();
    }
}