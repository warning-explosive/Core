namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;

    internal interface IModelProvider
    {
        IReadOnlyCollection<EnumTypeInfo> Enums { get; }

        IReadOnlyDictionary<Type, ITableInfo> Tables { get; }

        IReadOnlyDictionary<string, IReadOnlyDictionary<string, ITableInfo>> TablesMap { get; }

        IEnumerable<ColumnInfo> Columns(Type type);

        IEnumerable<ColumnInfo> Columns(ITableInfo table);

        string TableName(Type type);

        string SchemaName(Type type);
    }
}