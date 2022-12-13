﻿namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Basics;

    /// <summary>
    /// IndexInfo
    /// </summary>
    public class IndexInfo : IModelInfo,
                             IEquatable<IndexInfo>,
                             ISafelyEquatable<IndexInfo>
    {
        /// <summary> .cctor </summary>
        /// <param name="table">Table</param>
        /// <param name="columns">Columns</param>
        /// <param name="unique">Unique</param>
        public IndexInfo(
            ITableInfo table,
            IReadOnlyCollection<ColumnInfo> columns,
            bool unique)
        {
            Table = table;
            Columns = columns;
            Unique = unique;
        }

        /// <summary>
        /// Table
        /// </summary>
        public ITableInfo Table { get; }

        /// <summary>
        /// Columns
        /// </summary>
        public IReadOnlyCollection<ColumnInfo> Columns { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name => string.Join(
            "__",
            Table.Name,
            string.Join("_", Columns.OrderBy(column => column.Name).Select(column => column.Name)));

        /// <summary>
        /// Unique
        /// </summary>
        public bool Unique { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left IndexInfo</param>
        /// <param name="right">Right IndexInfo</param>
        /// <returns>equals</returns>
        public static bool operator ==(IndexInfo? left, IndexInfo? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left IndexInfo</param>
        /// <param name="right">Right IndexInfo</param>
        /// <returns>not equals</returns>
        public static bool operator !=(IndexInfo? left, IndexInfo? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        [SuppressMessage("Analysis", "CA1308", Justification = "sql script readability")]
        public override int GetHashCode()
        {
            return Columns
                .OrderBy(column => column.Name)
                .Aggregate(HashCode.Combine(Table, Unique), HashCode.Combine);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(IndexInfo? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(IndexInfo other)
        {
            return Table == other.Table
                   && Unique == other.Unique
                   && Columns.OrderBy(column => column.Name).SequenceEqual(other.Columns.OrderBy(column => column.Name));
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Table.Schema}.{Table.Name}.{Name} ({Unique})";
        }
    }
}