namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Abstractions;
    using Basics;

    /// <summary>
    /// QueryParameterExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class QueryParameterExpression : IIntermediateExpression,
                                            IEquatable<QueryParameterExpression>,
                                            ISafelyEquatable<QueryParameterExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">Item type</param>
        /// <param name="name">Query parameter name</param>
        /// <param name="value">Query parameter value</param>
        public QueryParameterExpression(Type itemType, string name, object? value)
        {
            ItemType = itemType;
            Name = name;
            Value = value;
        }

        /// <inheritdoc />
        public Type ItemType { get; }

        /// <summary>
        /// Query parameter name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Query parameter value
        /// </summary>
        public object? Value { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left QueryParameterExpression</param>
        /// <param name="right">Right QueryParameterExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(QueryParameterExpression? left, QueryParameterExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left QueryParameterExpression</param>
        /// <param name="right">Right QueryParameterExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(QueryParameterExpression? left, QueryParameterExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(ItemType, Name, Value);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(QueryParameterExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(QueryParameterExpression other)
        {
            return ItemType == other.ItemType
                   && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase)
                   && Value?.Equals(other.Value) == true;
        }

        #endregion
    }
}