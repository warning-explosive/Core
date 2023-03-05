namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using Basics.Enumerations;

    /// <summary>
    /// OrderByExpressionExpression
    /// </summary>
    public class OrderByExpressionExpression : ISqlExpression,
                                               IApplicable<ColumnExpression>,
                                               IApplicable<RenameExpression>,
                                               IApplicable<BinaryExpression>,
                                               IApplicable<ConditionalExpression>,
                                               IApplicable<MethodCallExpression>,
                                               IApplicable<QueryParameterExpression>,
                                               IApplicable<SpecialExpression>,
                                               IApplicable<UnaryExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="expression">Expression</param>
        /// <param name="orderingDirection">Ordering direction</param>
        public OrderByExpressionExpression(
            ISqlExpression expression,
            EnOrderingDirection orderingDirection)
        {
            Expression = expression;
            OrderingDirection = orderingDirection;
        }

        internal OrderByExpressionExpression(EnOrderingDirection orderingDirection)
            : this(null!, orderingDirection)
        {
        }

        /// <summary>
        /// Expression
        /// </summary>
        public ISqlExpression Expression { get; private set; }

        /// <summary>
        /// Ordering direction
        /// </summary>
        public EnOrderingDirection OrderingDirection { get; }

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, ColumnExpression expression)
        {
            ApplyExpression(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, RenameExpression expression)
        {
            ApplyExpression(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, BinaryExpression expression)
        {
            ApplyExpression(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ConditionalExpression expression)
        {
            ApplyExpression(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, MethodCallExpression expression)
        {
            ApplyExpression(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, QueryParameterExpression expression)
        {
            ApplyExpression(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, SpecialExpression expression)
        {
            ApplyExpression(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, UnaryExpression expression)
        {
            ApplyExpression(expression);
        }

        private void ApplyExpression(ISqlExpression expression)
        {
            if (Expression != null)
            {
                throw new InvalidOperationException("Order by expression source has already been set");
            }

            Expression = expression;
        }

        #endregion
    }
}