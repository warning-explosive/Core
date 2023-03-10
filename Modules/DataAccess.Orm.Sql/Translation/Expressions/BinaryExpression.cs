namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// BinaryExpression
    /// </summary>
    public class BinaryExpression : ITypedSqlExpression,
                                    IApplicable<ColumnExpression>,
                                    IApplicable<ConditionalExpression>,
                                    IApplicable<BinaryExpression>,
                                    IApplicable<UnaryExpression>,
                                    IApplicable<ParameterExpression>,
                                    IApplicable<QueryParameterExpression>,
                                    IApplicable<SpecialExpression>,
                                    IApplicable<MethodCallExpression>,
                                    IApplicable<ProjectionExpression>,
                                    IApplicable<FilterExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="operator">Operator</param>
        /// <param name="left">Left expression</param>
        /// <param name="right">Right expression</param>
        public BinaryExpression(
            Type type,
            BinaryOperator @operator,
            ISqlExpression left,
            ISqlExpression right)
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
        public ISqlExpression Left { get; private set; }

        /// <summary>
        /// Right expression
        /// </summary>
        public ISqlExpression Right { get; private set; }

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, ColumnExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ConditionalExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, BinaryExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, UnaryExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ParameterExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, QueryParameterExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, SpecialExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, MethodCallExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ProjectionExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, FilterExpression expression)
        {
            ApplySource(expression);
        }

        private void ApplySource(ISqlExpression expression)
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

            throw new InvalidOperationException("Source expression has already been set");
        }

        #endregion
    }
}