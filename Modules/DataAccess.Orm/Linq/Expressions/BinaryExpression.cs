namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using Abstractions;
    using Basics;

    /// <summary>
    /// BinaryExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class BinaryExpression : IIntermediateExpression,
                                    IEquatable<BinaryExpression>,
                                    ISafelyEquatable<BinaryExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">Item type</param>
        /// <param name="operator">Operator</param>
        /// <param name="left">Left IIntermediateExpression</param>
        /// <param name="right">Right IIntermediateExpression</param>
        public BinaryExpression(
            Type itemType,
            ExpressionType @operator,
            IIntermediateExpression left,
            IIntermediateExpression right)
        {
            ItemType = itemType;
            Operator = @operator;
            Left = left;
            Right = right;
        }

        internal BinaryExpression(Type itemType, ExpressionType @operator)
        {
            ItemType = itemType;
            Operator = @operator;
        }

        /// <inheritdoc />
        public Type ItemType { get; }

        /// <summary>
        /// Binary operator
        /// </summary>
        public ExpressionType Operator { get; }

        /// <summary>
        /// Left expression
        /// </summary>
        public IIntermediateExpression Left { get; private set; } = null!;

        /// <summary>
        /// Right expression
        /// </summary>
        public IIntermediateExpression Right { get; private set; } = null!;

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
            return HashCode.Combine(ItemType, Operator, Left, Right);
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
            return ItemType == other.ItemType
                   && Operator == other.Operator
                   && Left.Equals(other.Left)
                   && Right.Equals(other.Right);
        }

        #endregion

        internal void Apply(IIntermediateExpression expression)
        {
            if (Left == null)
            {
                Left = expression;
            }
            else if (Right == null)
            {
                Right = expression;
            }
        }
    }
}