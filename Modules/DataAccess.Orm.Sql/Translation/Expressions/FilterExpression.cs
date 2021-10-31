namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using Api.Exceptions;
    using Basics;

    /// <summary>
    /// FilterExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class FilterExpression : IIntermediateExpression,
                                    IEquatable<FilterExpression>,
                                    ISafelyEquatable<FilterExpression>,
                                    IApplicable<ProjectionExpression>,
                                    IApplicable<JoinExpression>,
                                    IApplicable<QuerySourceExpression>,
                                    IApplicable<QueryParameterExpression>,
                                    IApplicable<ParameterExpression>,
                                    IApplicable<BinaryExpression>,
                                    IApplicable<ConditionalExpression>,
                                    IApplicable<SimpleBindingExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="source">Source expression</param>
        /// <param name="predicate">Predicate expression</param>
        public FilterExpression(
            Type type,
            IIntermediateExpression source,
            IIntermediateExpression predicate)
        {
            Type = type;
            Source = source;
            Predicate = predicate;
        }

        internal FilterExpression(Type type)
            : this(type, null!, null!)
        {
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Source expression
        /// </summary>
        public IIntermediateExpression Source { get; private set; }

        /// <summary>
        /// Predicate expression
        /// </summary>
        public IIntermediateExpression Predicate { get; private set; }

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
            return HashCode.Combine(Type, Source, Predicate);
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
                   && Predicate.Equals(other.Predicate);
        }

        #endregion

        /// <inheritdoc />
        public Expression AsExpressionTree()
        {
            throw new TranslationException(nameof(FilterExpression) + "." + nameof(AsExpressionTree));
        }

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, QueryParameterExpression expression)
        {
            ApplyBinding(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ParameterExpression expression)
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
        public void Apply(TranslationContext context, SimpleBindingExpression expression)
        {
            ApplyBinding(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ProjectionExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, JoinExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, QuerySourceExpression expression)
        {
            ApplySource(expression);
        }

        private void ApplySource(IIntermediateExpression expression)
        {
            if (Source == null)
            {
                Source = expression;
            }
        }

        private void ApplyBinding(IIntermediateExpression expression)
        {
            if (Source is JoinExpression join)
            {
                expression = new ReplaceJoinBindingsVisitor(join).Visit(expression);
            }
            else if (Source is ProjectionExpression projection)
            {
                expression = new ReplaceFilterExpressionVisitor(projection).Visit(expression);

                if (projection.Source is JoinExpression projectionJoin)
                {
                    expression = new ReplaceJoinBindingsVisitor(projectionJoin).Visit(expression);
                }
            }

            Predicate = Predicate != null
                ? new BinaryExpression(typeof(bool), BinaryOperator.AndAlso, Predicate, expression)
                : expression;
        }

        #endregion
    }
}