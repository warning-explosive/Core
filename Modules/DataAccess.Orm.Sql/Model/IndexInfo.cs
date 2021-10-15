namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// IndexInfo
    /// </summary>
    public class IndexInfo : IModelInfo
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="columns">Columns</param>
        /// <param name="unique">Unique</param>
        public IndexInfo(
            string schema,
            Type table,
            IReadOnlyCollection<ColumnInfo> columns,
            bool unique)
        {
            Schema = schema;
            Columns = columns;
            Unique = unique;
            Table = table;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Table
        /// </summary>
        public Type Table { get; }

        /// <summary>
        /// Columns
        /// </summary>
        public IReadOnlyCollection<ColumnInfo> Columns { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name => string.Join("_", Columns.Select(column => column.Name).OrderBy(column => column));

        /// <summary>
        /// Unique
        /// </summary>
        public bool Unique { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Schema}.{Table.Name}.{Name} ({Unique})";
        }
    }
}