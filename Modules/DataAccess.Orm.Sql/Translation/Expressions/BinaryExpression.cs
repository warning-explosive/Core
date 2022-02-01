namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using Basics;

    /// <summary>
    /// BinaryExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class BinaryExpression : IIntermediateExpression,
                                    IEquatable<BinaryExpression>,
                                    ISafelyEquatable<BinaryExpression>,
                                    IApplicable<IIntermediateExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="operator">Operator</param>
        /// <param name="left">Left expression</param>
        /// <param name="right">Right expression</param>
        public BinaryExpression(
            Type type,
            BinaryOperator @operator,
            IIntermediateExpression left,
            IIntermediateExpression right)
        {
            Type = type;
            Operator = @operator;
            Left = left;
            Right = right;
        }

        internal BinaryExpression(Type type, BinaryOperator @operator)
            : this(type, @operator, null!, null!)
        {
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Binary operator
        /// </summary>
        public BinaryOperator Operator { get; }

        /// <summary>
        /// Left expression
        /// </summary>
        public IIntermediateExpression Left { get; private set; }

        /// <summary>
        /// Right expression
        /// </summary>
        public IIntermediateExpression Right { get; private set; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left BinaryExpression</param>
        /// <param name="right">Right BinaryExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(BinaryExpression? left, BinaryExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left BinaryExpression</param>
        /// <param name="right">Right BinaryExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(BinaryExpression? left, BinaryExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Operator, Left, Right);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(BinaryExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(BinaryExpression other)
        {
            return Type == other.Type
                   && Operator == other.Operator
                   && Left.Equals(other.Left)
                   && Right.Equals(other.Right);
        }

        #endregion

        /// <inheritdoc />
        public Expression AsExpressionTree()
        {
            return System.Linq.Expressions.Expression.MakeBinary(Operator.AsExpressionType(), Left.AsExpressionTree(), Right.AsExpressionTree());
        }

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, IIntermediateExpression expression)
        {
            if (expression is QueryParameterExpression
                && Operator == BinaryOperator.Contains
                && Right == null)
            {
                Right = expression;
                return;
            }

            if (Left == null)
            {
                Left = expression;
                return;
            }

            if (Right == null)
            {
                Right = expression;
                return;
            }

            throw new InvalidOperationException("Binary expression sources have already been set");
        }

        #endregion
    }
}