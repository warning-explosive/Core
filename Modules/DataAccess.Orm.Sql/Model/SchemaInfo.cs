namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Basics;

    /// <summary>
    /// SchemaInfo
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class SchemaInfo : IModelInfo,
                              IEquatable<SchemaInfo>,
                              ISafelyEquatable<SchemaInfo>
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="tables">Tables</param>
        /// <param name="views">Views</param>
        public SchemaInfo(
            string schema,
            IReadOnlyCollection<TableInfo> tables,
            IReadOnlyCollection<ViewInfo> views)
        {
            Schema = schema;
            Tables = tables;
            Views = views;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Tables
        /// </summary>
        public IReadOnlyCollection<TableInfo> Tables { get; }

        /// <summary>
        /// Views
        /// </summary>
        public IReadOnlyCollection<ViewInfo> Views { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left SchemaInfo</param>
        /// <param name="right">Right SchemaInfo</param>
        /// <returns>equals</returns>
        public static bool operator ==(SchemaInfo? left, SchemaInfo? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left SchemaInfo</param>
        /// <param name="right">Right SchemaInfo</param>
        /// <returns>not equals</returns>
        public static bool operator !=(SchemaInfo? left, SchemaInfo? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        [SuppressMessage("Analysis", "CA1308", Justification = "sql script readability")]
        public override int GetHashCode()
        {
            return Tables.OrderBy(column => column.Type).Cast<object>()
                .Concat(Views.OrderBy(view => view.Type))
                .Aggregate(Schema.GetHashCode(StringComparison.OrdinalIgnoreCase), HashCode.Combine);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(SchemaInfo? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(SchemaInfo other)
        {
            return Schema.Equals(other.Schema, StringComparison.OrdinalIgnoreCase)
                   && Tables.OrderBy(table => table.Type).SequenceEqual(other.Tables.OrderBy(table => table.Type))
                   && Views.OrderBy(view => view.Type).SequenceEqual(other.Views.OrderBy(view => view.Type));
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return Schema;
        }
    }
}