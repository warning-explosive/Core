namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using System.Collections.Generic;
    using Basics;

    /// <summary>
    /// SchemaNode
    /// </summary>
    public class SchemaNode : IEquatable<SchemaNode>,
                              ISafelyEquatable<SchemaNode>
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="types">Types</param>
        /// <param name="tables">Tables</param>
        /// <param name="views">Views</param>
        /// <param name="indexes">Indexes</param>
        public SchemaNode(string schema,
            IReadOnlyCollection<EnumTypeNode> types,
            IReadOnlyCollection<TableNode> tables,
            IReadOnlyCollection<ViewNode> views,
            IReadOnlyCollection<IndexNode> indexes)
        {
            Schema = schema;
            Types = types;
            Tables = tables;
            Views = views;
            Indexes = indexes;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Types
        /// </summary>
        public IReadOnlyCollection<EnumTypeNode> Types { get; }

        /// <summary>
        /// Tables
        /// </summary>
        public IReadOnlyCollection<TableNode> Tables { get; }

        /// <summary>
        /// Views
        /// </summary>
        public IReadOnlyCollection<ViewNode> Views { get; }

        /// <summary>
        /// Indexes
        /// </summary>
        public IReadOnlyCollection<IndexNode> Indexes { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left SchemaNode</param>
        /// <param name="right">Right SchemaNode</param>
        /// <returns>equals</returns>
        public static bool operator ==(SchemaNode? left, SchemaNode? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left SchemaNode</param>
        /// <param name="right">Right SchemaNode</param>
        /// <returns>not equals</returns>
        public static bool operator !=(SchemaNode? left, SchemaNode? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Schema.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(SchemaNode? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(SchemaNode other)
        {
            return Schema.Equals(other.Schema, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return Schema;
        }
    }
}