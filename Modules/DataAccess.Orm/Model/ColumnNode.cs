namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Basics;

    /// <summary>
    /// ColumnNode
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class ColumnNode : IEquatable<ColumnNode>,
                              ISafelyEquatable<ColumnNode>
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="column">Column</param>
        /// <param name="dataType">Data type</param>
        /// <param name="constraints">Constraints</param>
        public ColumnNode(
            string schema,
            string table,
            string column,
            string dataType,
            IReadOnlyCollection<string> constraints)
        {
            Schema = schema;
            Table = table;
            Column = column;
            DataType = dataType;
            Constraints = constraints;
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
        /// Column
        /// </summary>
        public string Column { get; }

        /// <summary>
        /// Data type
        /// </summary>
        public string DataType { get; }

        /// <summary>
        /// Constraints
        /// </summary>
        public IReadOnlyCollection<string> Constraints { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left ColumnNode</param>
        /// <param name="right">Right ColumnNode</param>
        /// <returns>equals</returns>
        public static bool operator ==(ColumnNode? left, ColumnNode? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left ColumnNode</param>
        /// <param name="right">Right ColumnNode</param>
        /// <returns>not equals</returns>
        public static bool operator !=(ColumnNode? left, ColumnNode? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        [SuppressMessage("Analysis", "CA1308", Justification = "sql script readability")]
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Schema.ToLowerInvariant(),
                Table.ToLowerInvariant(),
                Column.ToLowerInvariant(),
                DataType.ToLowerInvariant(),
                Constraints.ToString(", ").ToLowerInvariant());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(ColumnNode? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(ColumnNode other)
        {
            return Schema.Equals(other.Schema, StringComparison.OrdinalIgnoreCase)
                   && Table.Equals(other.Table, StringComparison.OrdinalIgnoreCase)
                   && Column.Equals(other.Column, StringComparison.OrdinalIgnoreCase)
                   && DataType.Equals(other.DataType, StringComparison.OrdinalIgnoreCase)
                   && Constraints.OrderBy(modifier => modifier).SequenceEqual(other.Constraints.OrderBy(modifier => modifier), StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Schema}.{Table}.{Column} ({new[] { DataType }.Concat(Constraints).ToString(", ")})";
        }
    }
}