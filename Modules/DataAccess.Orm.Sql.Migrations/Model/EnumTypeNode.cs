namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
{
    using System;
    using System.Collections.Generic;
    using Basics;

    /// <summary>
    /// EnumTypeNode
    /// </summary>
    public class EnumTypeNode : IEquatable<EnumTypeNode>,
                                ISafelyEquatable<EnumTypeNode>
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="type">Type</param>
        /// <param name="values">Values</param>
        public EnumTypeNode(
            string schema,
            string type,
            IReadOnlyCollection<string> values)
        {
            Schema = schema;
            Type = type;
            Values = values;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Values
        /// </summary>
        public IReadOnlyCollection<string> Values { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left EnumTypeNode</param>
        /// <param name="right">Right EnumTypeNode</param>
        /// <returns>equals</returns>
        public static bool operator ==(EnumTypeNode? left, EnumTypeNode? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left EnumTypeNode</param>
        /// <param name="right">Right EnumTypeNode</param>
        /// <returns>not equals</returns>
        public static bool operator !=(EnumTypeNode? left, EnumTypeNode? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Schema.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Type.GetHashCode(StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(EnumTypeNode? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(EnumTypeNode other)
        {
            return Schema.Equals(other.Schema, StringComparison.OrdinalIgnoreCase)
                && Type.Equals(other.Type, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Schema}.{Type} ({Values.ToString(", ")})";
        }
    }
}