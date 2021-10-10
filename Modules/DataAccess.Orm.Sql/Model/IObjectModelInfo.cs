namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// IObjectModelInfo
    /// </summary>
    public interface IObjectModelInfo : IModelInfo
    {
        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Type
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Columns
        /// </summary>
        IReadOnlyDictionary<string, ColumnInfo> Columns { get; }

        /// <summary>
        /// Indexes
        /// </summary>
        IReadOnlyDictionary<string, IndexInfo> Indexes { get; }
    }
}