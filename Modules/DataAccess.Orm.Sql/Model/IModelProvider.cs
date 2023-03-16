namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;

    internal interface IModelProvider
    {
        IReadOnlyCollection<EnumTypeInfo> Enums { get; }

        IReadOnlyDictionary<Type, ITableInfo> Tables { get; }

        IReadOnlyDictionary<string, IReadOnlyDictionary<string, ITableInfo>> TablesMap { get; }

        IReadOnlyDictionary<string, ColumnInfo> Columns(Type type);

        string TableName(Type type);

        string SchemaName(Type type);
    }
}