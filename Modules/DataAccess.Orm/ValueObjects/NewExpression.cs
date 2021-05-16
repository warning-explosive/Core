namespace SpaceEngineers.Core.DataAccess.Orm.ValueObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Abstractions;
    using Basics;

    /// <summary>
    /// NewExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class NewExpression : IIntermediateExpression,
                                 IEquatable<NewExpression>,
                                 ISafelyEquatable<NewExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">Item type</param>
        public NewExpression(Type itemType)
        {
            ItemType = itemType;
        }

        /// <inheritdoc />
        public Type ItemType { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left NewExpression</param>
        /// <param name="right">Right NewExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(NewExpression? left, NewExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left NewExpression</param>
        /// <param name="right">Right NewExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(NewExpression? left, NewExpression? right)
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
        public bool Equals(NewExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(NewExpression other)
        {
            return ItemType == other.ItemType;
        }

        #endregion
    }
}