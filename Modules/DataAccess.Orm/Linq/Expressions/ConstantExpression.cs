namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Abstractions;
    using Basics;

    /// <summary>
    /// ConstantExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class ConstantExpression : IIntermediateExpression,
                                      IEquatable<ConstantExpression>,
                                      ISafelyEquatable<ConstantExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">Item type</param>
        /// <param name="value">Constant value</param>
        public ConstantExpression(Type itemType, object? value)
        {
            ItemType = itemType;
            Value = value;
        }

        /// <inheritdoc />
        public Type ItemType { get; }

        /// <summary>
        /// Constant value
        /// </summary>
        public object? Value { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left ConstantExpression</param>
        /// <param name="right">Right ConstantExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(ConstantExpression? left, ConstantExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left ConstantExpression</param>
        /// <param name="right">Right ConstantExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(ConstantExpression? left, ConstantExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(ItemType, Value);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(ConstantExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(ConstantExpression other)
        {
            return ItemType == other.ItemType
                   && Value?.Equals(other.Value) == true;
        }

        #endregion
    }
}