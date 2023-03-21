namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// JsonAttributeExpression
    /// </summary>
    public class JsonAttributeExpression : ISqlExpression,
                                           IApplicable<ColumnExpression>,
                                           IApplicable<JsonAttributeExpression>,
                                           IApplicable<ParameterExpression>,
                                           IApplicable<QueryParameterExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="source">Source</param>
        /// <param name="accessor">Accessor</param>
        public JsonAttributeExpression(
            Type type,
            ISqlExpression source,
            ISqlExpression accessor)
        {
            Type = type;
            Source = source;
            Accessor = accessor;
        }

        internal JsonAttributeExpression(Type type)
            : this(type, null!, null!)
        {
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Source
        /// </summary>
        public ISqlExpression Source { get; private set; }

        /// <summary>
        /// Json attribute accessor
        /// </summary>
        public ISqlExpression Accessor { get; private set; }

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
        public void Apply(TranslationContext context, ParameterExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, QueryParameterExpression expression)
        {
            ApplySource(expression);
        }

        private void ApplySource(ISqlExpression expression)
        {
            if (Source == null)
            {
                Source = expression;
                return;
            }

            if (Accessor == null)
            {
                Accessor = expression;
                return;
            }

            throw new InvalidOperationException("Source expression has already been set");
        }

        #endregion
    }
}