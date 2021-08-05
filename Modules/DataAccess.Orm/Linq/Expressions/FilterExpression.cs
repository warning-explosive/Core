namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using Abstractions;
    using Basics;
    using Visitors;

    /// <summary>
    /// FilterExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class FilterExpression : ISubsequentIntermediateExpression,
                                    IEquatable<FilterExpression>,
                                    ISafelyEquatable<FilterExpression>,
                                    IApplicable<ProjectionExpression>,
                                    IApplicable<QuerySourceExpression>,
                                    IApplicable<QueryParameterExpression>,
                                    IApplicable<ParameterExpression>,
                                    IApplicable<BinaryExpression>,
                                    IApplicable<ConditionalExpression>,
                                    IApplicable<SimpleBindingExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="source">Source</param>
        /// <param name="expression">Expression</param>
        public FilterExpression(
            Type type,
            IIntermediateExpression source,
            IIntermediateExpression expression)
        {
            Type = type;
            Source = source;
            Expression = expression;
        }

        internal FilterExpression(Type type)
            : this(type, null !, null !)
        {
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Source expression which we want to filter
        /// </summary>
        public IIntermediateExpression Source { get; private set; } = null!;

        /// <summary>
        /// Filtering expression
        /// </summary>
        public IIntermediateExpression Expression { get; private set; } = null!;

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left FilterExpression</param>
        /// <param name="right">Right FilterExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(FilterExpression? left, FilterExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left FilterExpression</param>
        /// <param name="right">Right FilterExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(FilterExpression? left, FilterExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Source, Expression);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(FilterExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(FilterExpression other)
        {
            return Type == other.Type
                   && Source.Equals(other.Source)
                   && Expression.Equals(other.Expression);
        }

        #endregion

        /// <inheritdoc />
        public Expression AsExpressionTree()
        {
            throw new NotImplementedException(nameof(FilterExpression) + "." + nameof(AsExpressionTree));
        }

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, ProjectionExpression projection)
        {
            Source = projection;
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, QuerySourceExpression querySource)
        {
            Source = querySource;
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, QueryParameterExpression queryParameter)
        {
            ApplyBinding(queryParameter);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ParameterExpression parameter)
        {
            ApplyBinding(parameter);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, BinaryExpression binary)
        {
            ApplyBinding(binary);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ConditionalExpression conditional)
        {
            ApplyBinding(conditional);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, SimpleBindingExpression binding)
        {
            ApplyBinding(binding);
        }

        private void ApplyBinding(IIntermediateExpression expression)
        {
            if (Source is ProjectionExpression projection)
            {
                expression = new ReplaceFilterExpressionVisitor(projection).Visit(expression);
            }

            Expression = Expression != null
                ? new BinaryExpression(typeof(bool), ExpressionType.AndAlso, Expression, expression)
                : expression;
        }

        #endregion
    }
}