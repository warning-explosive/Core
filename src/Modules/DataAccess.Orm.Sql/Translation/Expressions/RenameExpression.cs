namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// RenameExpression
    /// </summary>
    public class RenameExpression : ITypedSqlExpression,
                                    IApplicable<ColumnExpression>,
                                    IApplicable<JsonAttributeExpression>,
                                    IApplicable<ParenthesesExpression>,
                                    IApplicable<BinaryExpression>,
                                    IApplicable<UnaryExpression>,
                                    IApplicable<ConditionalExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="source">Source expression</param>
        public RenameExpression(Type type, string name, ISqlExpression source)
        {
            Type = type;
            Name = name;
            Source = source;
        }

        internal RenameExpression(Type type, string name)
            : this(type, name, null!)
        {
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Source
        /// </summary>
        public ISqlExpression Source { get; private set; }

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
        public void Apply(TranslationContext context, ConditionalExpression expression)
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