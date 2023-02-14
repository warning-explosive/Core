namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// IModelProvider
    /// </summary>
    public interface IModelProvider
    {
        /// <summary>
        /// Enums
        /// </summary>
        IReadOnlyCollection<EnumTypeInfo> Enums { get; }

        /// <summary>
        /// Tables
        /// </summary>
        IReadOnlyDictionary<Type, ITableInfo> Tables { get; }

        /// <summary>
        /// Tables map
        /// [schema] -> [table] -> [info]
        /// </summary>
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, ITableInfo>> TablesMap { get; }

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