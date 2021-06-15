namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Abstractions;
    using Basics;

    /// <summary>
    /// QuerySourceExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class QuerySourceExpression : IIntermediateExpression,
                                         IEquatable<QuerySourceExpression>,
                                         ISafelyEquatable<QuerySourceExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">Item type</param>
        public QuerySourceExpression(Type itemType)
        {
            ItemType = itemType;
        }

        /// <inheritdoc />
        public Type ItemType { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left QuerySourceExpression</param>
        /// <param name="right">Right QuerySourceExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(QuerySourceExpression? left, QuerySourceExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left QuerySourceExpression</param>
        /// <param name="right">Right QuerySourceExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(QuerySourceExpression? left, QuerySourceExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(ItemType);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(QuerySourceExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(QuerySourceExpression other)
        {
            return ItemType == other.ItemType;
        }

        #endregion
    }
}