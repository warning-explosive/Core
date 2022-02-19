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
        /// Tables map
        /// </summary>
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, ITableInfo>> TablesMap { get; }

        /// <summary>
        /// Tables
        /// </summary>
        IReadOnlyDictionary<Type, ITableInfo> Tables { get; }

        /// <summary>
        /// Mtm-tables
        /// </summary>
        public IReadOnlyDictionary<Type, MtmTableInfo> MtmTables { get; }

        /// <summary>
        /// Gets tables for specified database entities
        /// </summary>
        /// <param name="databaseEntities">Database entities</param>
        /// <returns>Tables</returns>
        public IEnumerable<ITableInfo> TablesFor(Type[] databaseEntities);

        /// <summary>
        /// Gets columns for specified type
        /// </summary>
        /// <param name="type">Table or projection type</param>
        /// <returns>Column infos</returns>
        IEnumerable<ColumnInfo> Columns(Type type);

        /// <summary>
        /// Gets columns for specified type
        /// </summary>
        /// <param name="table">ITableInfo</param>
        /// <returns>Column infos</returns>
        IEnumerable<ColumnInfo> Columns(ITableInfo table);

        /// <summary>
        /// Gets table name
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Schema name</returns>
        string TableName(Type type);

        /// <summary>
        /// Gets schema name
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Schema name</returns>
        string SchemaName(Type type);
    }
}