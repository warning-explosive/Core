namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Linq.Expressions;
    using Api.Exceptions;
    using Basics;

    /// <summary>
    /// NewExpression
    /// </summary>
    public class NewExpression : ISqlExpression,
                                 IEquatable<NewExpression>,
                                 ISafelyEquatable<NewExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        public NewExpression(Type type)
        {
            Type = type;
        }

        /// <inheritdoc />
        public Type Type { get; }

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
            return HashCode.Combine(Type);
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
            return Type == other.Type;
        }

        #endregion

        /// <inheritdoc />
        public Expression AsExpressionTree()
        {
            throw new TranslationException(nameof(NewExpression) + "." + nameof(AsExpressionTree));
        }
    }
}