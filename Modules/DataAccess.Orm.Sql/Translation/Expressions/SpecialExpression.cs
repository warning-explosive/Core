namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using Basics;

    /// <summary>
    /// SpecialExpression
    /// </summary>
    public class SpecialExpression : ISqlExpression,
                                     IEquatable<SpecialExpression>,
                                     ISafelyEquatable<SpecialExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="text">Text</param>
        public SpecialExpression(Type type, string text)
        {
            Type = type;
            Text = text;
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left SpecialExpression</param>
        /// <param name="right">Right SpecialExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(SpecialExpression? left, SpecialExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left SpecialExpression</param>
        /// <param name="right">Right SpecialExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(SpecialExpression? left, SpecialExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Type,
                Text.GetHashCode(StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(SpecialExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(SpecialExpression other)
        {
            return Type == other.Type
                   && Text.Equals(other.Text, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}