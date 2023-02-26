﻿namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// ITableInfo
    /// </summary>
    public interface ITableInfo : IModelInfo
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

        /// <summary>
        /// Does table represent mtm table
        /// </summary>
        bool IsMtmTable { get; }
    }
}