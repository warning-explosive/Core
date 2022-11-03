namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Basics;

    /// <summary>
    /// TableNode
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class TableNode : IEquatable<TableNode>,
                             ISafelyEquatable<TableNode>
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="columns">Columns</param>
        public TableNode(
            string schema,
            string table,
            IReadOnlyCollection<ColumnNode> columns)
        {
            Schema = schema;
            Table = table;
            Columns = columns;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Table
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// Columns
        /// </summary>
        public IReadOnlyCollection<ColumnNode> Columns { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left TableNode</param>
        /// <param name="right">Right TableNode</param>
        /// <returns>equals</returns>
        public static bool operator ==(TableNode? left, TableNode? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left TableNode</param>
        /// <param name="right">Right TableNode</param>
        /// <returns>not equals</returns>
        public static bool operator !=(TableNode? left, TableNode? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        [SuppressMessage("Analysis", "CA1308", Justification = "sql script readability")]
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Schema.ToLowerInvariant(),
                Table.ToLowerInvariant());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(TableNode? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(TableNode other)
        {
            return Schema.Equals(other.Schema, StringComparison.OrdinalIgnoreCase)
                   && Table.Equals(other.Table, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Schema}.{Table}";
        }
    }
}