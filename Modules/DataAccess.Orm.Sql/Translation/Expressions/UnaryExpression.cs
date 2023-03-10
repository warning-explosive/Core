namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// UnaryExpression
    /// </summary>
    public class UnaryExpression : ITypedSqlExpression,
                                   IApplicable<ColumnExpression>,
                                   IApplicable<ConditionalExpression>,
                                   IApplicable<BinaryExpression>,
                                   IApplicable<UnaryExpression>,
                                   IApplicable<ParameterExpression>,
                                   IApplicable<QueryParameterExpression>,
                                   IApplicable<SpecialExpression>,
                                   IApplicable<MethodCallExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="operator">Operator</param>
        /// <param name="source">Source expression</param>
        public UnaryExpression(
            Type type,
            UnaryOperator @operator,
            ISqlExpression source)
        {
            Type = type;
            Operator = @operator;
            Source = source;
        }

        internal UnaryExpression(Type type, UnaryOperator @operator)
            : this(type, @operator, null!)
        {
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Binary operator
        /// </summary>
        public UnaryOperator Operator { get; }

        /// <summary>
        /// Source expression
        /// </summary>
        public ISqlExpression Source { get; private set; }

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

        private void ApplySource(ISqlExpression expression)
        {
            if (Source != null)
            {
                throw new InvalidOperationException("Source expression has already been set");
            }

            Source = expression;
        }

        #endregion
    }
}