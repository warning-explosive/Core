namespace SpaceEngineers.Core.DataAccess.Orm.ValueObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Abstractions;
    using Basics;

    /// <summary>
    /// ParameterExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class ParameterExpression : IIntermediateExpression,
                                       IEquatable<ParameterExpression>,
                                       ISafelyEquatable<ParameterExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">Item type</param>
        /// <param name="name">Name</param>
        public ParameterExpression(Type itemType, string name)
        {
            ItemType = itemType;
            Name = name;
        }

        /// <inheritdoc />
        public Type ItemType { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left ParameterExpression</param>
        /// <param name="right">Right ParameterExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(ParameterExpression? left, ParameterExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left ParameterExpression</param>
        /// <param name="right">Right ParameterExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(ParameterExpression? left, ParameterExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(ItemType, Name);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(ParameterExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(ParameterExpression other)
        {
            return ItemType == other.ItemType
                   && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}