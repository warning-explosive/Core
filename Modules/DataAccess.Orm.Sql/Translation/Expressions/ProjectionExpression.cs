namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;

    /// <summary>
    /// ProjectionExpression
    /// </summary>
    public class ProjectionExpression : ISqlExpression,
                                        IApplicable<FilterExpression>,
                                        IApplicable<JoinExpression>,
                                        IApplicable<NamedSourceExpression>,
                                        IApplicable<NewExpression>,
                                        IApplicable<ColumnExpression>,
                                        IApplicable<RenameExpression>,
                                        IApplicable<BinaryExpression>,
                                        IApplicable<UnaryExpression>,
                                        IApplicable<ConditionalExpression>,
                                        IApplicable<MethodCallExpression>
    {
        private readonly List<ISqlExpression> _expressions;

        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="source">Source expression</param>
        /// <param name="expressions">Expressions</param>
        public ProjectionExpression(
            Type type,
            ISqlExpression source,
            IEnumerable<ISqlExpression> expressions)
        {
            Type = type;
            Source = source;
            IsProjectionToClass = type.IsClass && !type.IsPrimitive() && !type.IsCollection();
            IsAnonymousProjection = type.IsAnonymous();

            _expressions = expressions.ToList();
        }

        internal ProjectionExpression(Type type)
            : this(type, null!, Array.Empty<ISqlExpression>())
        {
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Is projection creates anonymous or user defined class
        /// </summary>
        public bool IsProjectionToClass { get; private set; }

        /// <summary>
        /// Is projection creates anonymous class
        /// </summary>
        public bool IsAnonymousProjection { get; private set; }

        /// <summary>
        /// Is projection takes distinct values
        /// </summary>
        public bool IsDistinct { get; set; }

        /// <summary>
        /// Source expression
        /// </summary>
        public ISqlExpression Source { get; private set; }

        /// <summary>
        /// Expressions
        /// </summary>
        public IReadOnlyCollection<ISqlExpression> Expressions => _expressions;

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, NewExpression expression)
        {
            IsProjectionToClass = true;
            IsAnonymousProjection = expression.Type.IsAnonymous();
        }

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
        public void Apply(TranslationContext context, UnaryExpression expression)
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
        public void Apply(TranslationContext context, FilterExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, JoinExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, NamedSourceExpression expression)
        {
            ApplySource(expression);
        }

        private void ApplyExpression(ISqlExpression expression)
        {
            if (Source is JoinExpression join)
            {
                expression = expression.ReplaceJoinExpressions(join, true);
            }

            if (expression is ParameterExpression)
            {
                return;
            }

            _expressions.Add(expression);
        }

        private void ApplySource(ISqlExpression expression)
        {
            if (Source != null)
            {
                throw new InvalidOperationException("Projection expression source has already been set");
            }

            Source = expression;
        }

        #endregion
    }
}