namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
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
        /// <param name="type">Type</param>
        /// <param name="value">Constant value</param>
        public ConstantExpression(Type type, object? value)
        {
            Type = type;
            Value = value;
        }

        /// <inheritdoc />
        public Type Type { get; }

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
            return HashCode.Combine(Type, Value);
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
            return Type == other.Type
                   && Value?.Equals(other.Value) == true;
        }

        #endregion

        /// <inheritdoc />
        public Expression AsExpressionTree()
        {
            return System.Linq.Expressions.Expression.Constant(Value, Type);
        }
    }
}