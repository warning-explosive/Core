namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using Basics;

    /// <summary>
    /// IndexNode
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class IndexNode : IEquatable<IndexNode>,
                             ISafelyEquatable<IndexNode>
    {
        private const char Separator = '_';

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
            return HashCode.Combine(ToString());
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
            return ToString().Equals(other.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        #endregion

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
            sb.Append(string.Join(Separator, Columns.OrderBy(it => it)));

            if (Unique)
            {
                sb.Append(Separator);
                sb.Append(Separator);
                sb.Append(nameof(Unique));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds IndexNode from actual index name
        /// </summary>
        /// <param name="schema">Schema</param>
        /// <param name="table">Table</param>
        /// <param name="name">Name</param>
        /// <returns>IndexNode</returns>
        public static IndexNode FromDb(string schema, string table, string name)
        {
            var parts = name.Split(new string(Separator, 2), StringSplitOptions.RemoveEmptyEntries);

            var columns = parts[0]
                .Split(Separator, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            var unique = name.Contains("__unique", StringComparison.OrdinalIgnoreCase);

            return new IndexNode(schema, table, columns, unique);
        }
    }
}