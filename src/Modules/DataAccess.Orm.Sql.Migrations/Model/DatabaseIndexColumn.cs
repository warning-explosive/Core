﻿namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
{
    using System;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model.Attributes;

    /// <summary>
    /// DatabaseIndexColumn
    /// </summary>
    [Schema(nameof(Migrations))]
    [Index(nameof(Schema), nameof(Table), nameof(Index), nameof(Column), Unique = true)]
    public record DatabaseIndexColumn : BaseSqlView<Guid>
    {
        /// <summary> .cctor </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="index">Index</param>
        /// <param name="unique">Unique</param>
        /// <param name="predicate">Partial index predicate</param>
        /// <param name="column">Column</param>
        /// <param name="isKeyColumn">Is key column</param>
        /// <param name="definition">Definition</param>
        public DatabaseIndexColumn(
            Guid primaryKey,
            string schema,
            string table,
            string index,
            bool unique,
            string? predicate,
            string column,
            bool isKeyColumn,
            string definition)
            : base(primaryKey)
        {
            Schema = schema;
            Table = table;
            Index = index;
            Unique = unique;
            Predicate = predicate;
            Column = column;
            IsKeyColumn = isKeyColumn;
            Definition = definition;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Table
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Index
        /// </summary>
        public string Index { get; set; }

        /// <summary>
        /// Unique
        /// </summary>
        public bool Unique { get; set; }

        /// <summary>
        /// Partial index predicate
        /// </summary>
        public string? Predicate { get; set; }

        /// <summary>
        /// Column
        /// </summary>
        public string Column { get; set; }

        /// <summary>
        /// Is key column or non-key (included)
        /// </summary>
        public bool IsKeyColumn { get; set; }

        /// <summary>
        /// Definition
        /// </summary>
        public string Definition { get; set; }
    }
}