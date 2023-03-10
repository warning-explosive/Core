﻿namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;

    /// <summary>
    /// IndexNode
    /// </summary>
    public class IndexNode : IEquatable<IndexNode>,
                             ISafelyEquatable<IndexNode>
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="columns">Columns</param>
        /// <param name="includedColumns">Included columns</param>
        /// <param name="unique">Unique</param>
        public IndexNode(
            string schema,
            string table,
            IReadOnlyCollection<string> columns,
            IReadOnlyCollection<string> includedColumns,
            bool unique)
        {
            Schema = schema;
            Table = table;
            Columns = columns;
            IncludedColumns = includedColumns;
            Unique = unique;
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
        /// Index
        /// </summary>
        public string Index => string.Join(
            "__",
            Table,
            string.Join("_", Columns.OrderBy(column => column)));

        /// <summary>
        /// Unique
        /// </summary>
        public bool Unique { get; }

        /// <summary>
        /// Columns
        /// </summary>
        public IReadOnlyCollection<string> Columns { get; }

        /// <summary>
        /// Included columns
        /// </summary>
        public IReadOnlyCollection<string> IncludedColumns { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left IndexNode</param>
        /// <param name="right">Right IndexNode</param>
        /// <returns>equals</returns>
        public static bool operator ==(IndexNode? left, IndexNode? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left IndexNode</param>
        /// <param name="right">Right IndexNode</param>
        /// <returns>not equals</returns>
        public static bool operator !=(IndexNode? left, IndexNode? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Schema.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Table.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Index.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Unique.Bit(),
                Columns.OrderBy(column => column).ToString(", ").GetHashCode(StringComparison.OrdinalIgnoreCase),
                IncludedColumns.OrderBy(column => column).ToString(", ").GetHashCode(StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(IndexNode? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(IndexNode other)
        {
            return Schema.Equals(other.Schema, StringComparison.OrdinalIgnoreCase)
                && Table.Equals(other.Table, StringComparison.OrdinalIgnoreCase)
                && Index.Equals(other.Index, StringComparison.OrdinalIgnoreCase)
                && Unique == other.Unique
                && Columns.OrderBy(column => column).SequenceEqual(other.Columns.OrderBy(column => column), StringComparer.OrdinalIgnoreCase)
                && IncludedColumns.OrderBy(column => column).SequenceEqual(other.IncludedColumns.OrderBy(column => column), StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Schema}.{Index}";
        }
    }
}