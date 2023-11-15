namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// ConditionalExpression
    /// </summary>
    public class ConditionalExpression : ITypedSqlExpression,
                                         IApplicable<ColumnExpression>,
                                         IApplicable<JsonAttributeExpression>,
                                         IApplicable<ConditionalExpression>,
                                         IApplicable<ParenthesesExpression>,
                                         IApplicable<BinaryExpression>,
                                         IApplicable<UnaryExpression>,
                                         IApplicable<ParameterExpression>,
                                         IApplicable<QueryParameterExpression>,
                                         IApplicable<SpecialExpression>,
                                         IApplicable<MethodCallExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="when">When expression</param>
        /// <param name="then">Then expression</param>
        /// <param name="else">Else expression</param>
        public ConditionalExpression(
            Type type,
            ISqlExpression when,
            ISqlExpression then,
            ISqlExpression @else)
        {
            Type = type;
            When = when;
            Then = then;
            Else = @else;
        }

        internal ConditionalExpression(Type type)
            : this(type, null!, null!, null!)
        {
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// When condition
        /// </summary>
        public ISqlExpression When { get; private set; }

        /// <summary>
        /// Then expression
        /// </summary>
        public ISqlExpression Then { get; private set; }

        /// <summary>
        /// Then expression
        /// </summary>
        public ISqlExpression Else { get; private set; }

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, ColumnExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, JsonAttributeExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ConditionalExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ParenthesesExpression expression)
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
            if (When == null)
            {
                When = expression;
                return;
            }

            if (Then == null)
            {
                Then = expression;
                return;
            }

            if (Else == null)
            {
                Else = expression;
                return;
            }

            throw new InvalidOperationException("Source expression has already been set");
        }

        #endregion
    }
}