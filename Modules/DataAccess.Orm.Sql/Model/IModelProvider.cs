namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IModelProvider
    /// </summary>
    public interface IModelProvider : IResolvable
    {
        /// <summary>
        /// Objects
        /// </summary>
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, IObjectModelInfo>> Objects { get; }

        /// <summary>
        /// Mtm tables
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyDictionary<Type, (Type Left, Type Right)>> MtmTables { get; }

        /// <summary>
        /// Gets objects for specified database entities
        /// </summary>
        /// <param name="databaseEntities">Database entities</param>
        /// <returns>Objects</returns>
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, IObjectModelInfo>> ObjectsFor(Type[] databaseEntities);
    }
}