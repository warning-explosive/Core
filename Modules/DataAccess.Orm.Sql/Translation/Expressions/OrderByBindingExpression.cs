namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using Basics;
    using Basics.Enumerations;

    /// <summary>
    /// OrderByBindingExpression
    /// </summary>
    public class OrderByBindingExpression : ISqlExpression,
                                            IEquatable<OrderByBindingExpression>,
                                            ISafelyEquatable<OrderByBindingExpression>,
                                            IApplicable<SimpleBindingExpression>,
                                            IApplicable<NamedBindingExpression>,
                                            IApplicable<BinaryExpression>,
                                            IApplicable<ConditionalExpression>,
                                            IApplicable<MethodCallExpression>,
                                            IApplicable<QueryParameterExpression>,
                                            IApplicable<SpecialExpression>,
                                            IApplicable<UnaryExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="binding">Binding</param>
        /// <param name="orderingDirection">Ordering direction</param>
        public OrderByBindingExpression(
            ISqlExpression binding,
            EnOrderingDirection orderingDirection)
        {
            Binding = binding;
            OrderingDirection = orderingDirection;
        }

        internal OrderByBindingExpression(EnOrderingDirection orderingDirection)
            : this(null!, orderingDirection)
        {
        }

        /// <inheritdoc />
        public Type Type => Binding.Type;

        /// <summary>
        /// Binding
        /// </summary>
        public ISqlExpression Binding { get; private set; }

        /// <summary>
        /// Ordering direction
        /// </summary>
        public EnOrderingDirection OrderingDirection { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left OrderByBindingExpression</param>
        /// <param name="right">Right OrderByBindingExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(OrderByBindingExpression? left, OrderByBindingExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left OrderByBindingExpression</param>
        /// <param name="right">Right OrderByBindingExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(OrderByBindingExpression? left, OrderByBindingExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Binding, OrderingDirection);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(OrderByBindingExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(OrderByBindingExpression other)
        {
            return Type == other.Type
                && Binding.Equals(other.Binding)
                && OrderingDirection == other.OrderingDirection;
        }

        #endregion

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, SimpleBindingExpression expression)
        {
            ApplyBinding(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, NamedBindingExpression expression)
        {
            ApplyBinding(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, BinaryExpression expression)
        {
            ApplyBinding(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ConditionalExpression expression)
        {
            ApplyBinding(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, MethodCallExpression expression)
        {
            ApplyBinding(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, QueryParameterExpression expression)
        {
            ApplyBinding(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, SpecialExpression expression)
        {
            ApplyBinding(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, UnaryExpression expression)
        {
            ApplyBinding(expression);
        }

        private void ApplyBinding(ISqlExpression expression)
        {
            if (Binding != null)
            {
                throw new InvalidOperationException("Order by binding expression source has already been set");
            }

            Binding = expression;
        }

        #endregion
    }
}