namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using Api.Exceptions;
    using Basics;

    /// <summary>
    /// JoinExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class JoinExpression : IIntermediateExpression,
                                  IEquatable<JoinExpression>,
                                  ISafelyEquatable<JoinExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="leftSource">Left source expression</param>
        /// <param name="rightSource">Right source expression</param>
        /// <param name="on">On expression</param>
        public JoinExpression(
            IIntermediateExpression leftSource,
            IIntermediateExpression rightSource,
            IIntermediateExpression on)
        {
            LeftSource = leftSource;
            RightSource = rightSource;
            On = on;
            Type = typeof(ValueTuple<,>).MakeGenericType(leftSource.Type, rightSource.Type);
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Left source expression
        /// </summary>
        public IIntermediateExpression LeftSource { get; }

        /// <summary>
        /// Right source expression
        /// </summary>
        public IIntermediateExpression RightSource { get; }

        /// <summary>
        /// On expression
        /// </summary>
        public IIntermediateExpression On { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left JoinExpression</param>
        /// <param name="right">Right JoinExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(JoinExpression? left, JoinExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left JoinExpression</param>
        /// <param name="right">Right JoinExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(JoinExpression? left, JoinExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, LeftSource, RightSource, On);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(JoinExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(JoinExpression other)
        {
            return Type == other.Type
                   && LeftSource.Equals(other.LeftSource)
                   && RightSource.Equals(other.RightSource)
                   && On.Equals(other.On);
        }

        #endregion

        /// <inheritdoc />
        public Expression AsExpressionTree()
        {
            throw new TranslationException(nameof(JoinExpression) + "." + nameof(AsExpressionTree));
        }
    }
}