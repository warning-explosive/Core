namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
{
    using System;
    using Basics;

    /// <summary>
    /// FunctionNode
    /// </summary>
    public class FunctionNode : IEquatable<FunctionNode>,
                                ISafelyEquatable<FunctionNode>
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="function">Function</param>
        /// <param name="definition">Definition</param>
        public FunctionNode(
            string schema,
            string function,
            string definition)
        {
            Schema = schema;
            Function = function;
            Definition = definition;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Function { get; }

        /// <summary>
        /// Definition
        /// </summary>
        public string Definition { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left FunctionNode</param>
        /// <param name="right">Right FunctionNode</param>
        /// <returns>equals</returns>
        public static bool operator ==(FunctionNode? left, FunctionNode? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left FunctionNode</param>
        /// <param name="right">Right FunctionNode</param>
        /// <returns>not equals</returns>
        public static bool operator !=(FunctionNode? left, FunctionNode? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Schema.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Function.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Definition.GetHashCode(StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(FunctionNode? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(FunctionNode other)
        {
            return Schema.Equals(other.Schema, StringComparison.OrdinalIgnoreCase)
                   && Function.Equals(other.Function, StringComparison.OrdinalIgnoreCase)
                   && Definition.Equals(other.Definition, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Schema}.{Function}";
        }
    }
}