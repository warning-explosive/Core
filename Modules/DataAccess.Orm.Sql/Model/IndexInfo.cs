namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Orm.Model;

    /// <summary>
    /// IndexInfo
    /// </summary>
    public class IndexInfo : IModelInfo
    {
        private const char Separator = '_';

        /// <summary> .cctor </summary>
        /// <param name="tableType">Table type</param>
        /// <param name="columns">Columns</param>
        /// <param name="unique">Unique</param>
        public IndexInfo(
            Type tableType,
            IReadOnlyCollection<ColumnInfo> columns,
            bool unique)
        {
            Columns = columns;
            Unique = unique;
            TableType = tableType;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema => TableType.SchemaName();

        /// <summary>
        /// Table
        /// </summary>
        public string Table => TableType.Name;

        /// <summary>
        /// Table type
        /// </summary>
        public Type TableType { get; }

        /// <summary>
        /// Columns
        /// </summary>
        public IReadOnlyCollection<ColumnInfo> Columns { get; }

        /// <summary>
        /// Unique
        /// </summary>
        public bool Unique { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(Schema);
            sb.Append(Separator);
            sb.Append(Separator);
            sb.Append(Table);
            sb.Append(Separator);
            sb.Append(Separator);
            sb.Append(string.Join(Separator, Columns.Select(column => column.Name).OrderBy(column => column)));

            if (Unique)
            {
                sb.Append(Separator);
                sb.Append(Separator);
                sb.Append(nameof(Unique));
            }

            return sb.ToString();
        }
    }
}