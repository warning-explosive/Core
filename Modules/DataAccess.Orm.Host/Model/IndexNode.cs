namespace SpaceEngineers.Core.DataAccess.Orm.Host.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Basics;

    /// <summary>
    /// IndexNode
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class IndexNode : IEquatable<IndexNode>,
                             ISafelyEquatable<IndexNode>
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="columns">Columns</param>
        /// <param name="unique">Unique</param>
        public IndexNode(
            string schema,
            string table,
            IReadOnlyCollection<string> columns,
            bool unique)
        {
            Schema = schema;
            Table = table;
            Columns = columns;
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
        /// Columns
        /// </summary>
        public IReadOnlyCollection<string> Columns { get; }

        /// <summary>
        /// Unique
        /// </summary>
        public bool Unique { get; }

        /// <summary>
        /// Index
        /// </summary>
        public string Index => string.Join("_", Columns.OrderBy(it => it));

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
        [SuppressMessage("Analysis", "CA1308", Justification = "sql script readability")]
        public override int GetHashCode()
        {
            return HashCode.Combine(Index.ToLowerInvariant());
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
            return Index.Equals(other.Index, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Schema}.{Table}.{Index}";
        }

        /// <summary>
        /// Builds IndexNode from actual index name
        /// </summary>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="name">Name</param>
        /// <param name="definition">Definition</param>
        /// <returns>IndexNode</returns>
        public static IndexNode FromDb(string schema, string table, string name, string definition)
        {
            var columns = name
                .Split("_", StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            var unique = definition.Contains("create unique index", StringComparison.OrdinalIgnoreCase);

            return new IndexNode(schema, table, columns, unique);
        }
    }
}