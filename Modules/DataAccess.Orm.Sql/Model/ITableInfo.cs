namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;

    internal interface ITableInfo : IModelInfo
    {
        public string Schema { get; }

        public string Name { get; }

        Type Type { get; }

        IReadOnlyDictionary<string, ColumnInfo> Columns { get; }

        IReadOnlyDictionary<string, IndexInfo> Indexes { get; }

        bool IsMtmTable { get; }
    }
}