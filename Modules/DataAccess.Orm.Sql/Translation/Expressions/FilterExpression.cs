namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using Basics;

    /// <summary>
    /// FilterExpression
    /// </summary>
    public class FilterExpression : ISqlExpression,
                                    IEquatable<FilterExpression>,
                                    ISafelyEquatable<FilterExpression>,
                                    IApplicable<ProjectionExpression>,
                                    IApplicable<JoinExpression>,
                                    IApplicable<QuerySourceExpression>,
                                    IApplicable<QueryParameterExpression>,
                                    IApplicable<ParameterExpression>,
                                    IApplicable<BinaryExpression>,
                                    IApplicable<UnaryExpression>,
                                    IApplicable<ConditionalExpression>,
                                    IApplicable<SimpleBindingExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="source">Source expression</param>
        /// <param name="predicate">Predicate expression</param>
        public FilterExpression(
            Type type,
            ISqlExpression source,
            ISqlExpression predicate)
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
        public ISqlExpression Source { get; private set; }

        /// <summary>
        /// Predicate expression
        /// </summary>
        public ISqlExpression Predicate { get; private set; }

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
        public void Apply(TranslationContext context, UnaryExpression expression)
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

        private void ApplySource(ISqlExpression expression)
        {
            if (Source != null)
            {
                throw new InvalidOperationException("Filter expression source has already been set");
            }

            Source = expression;
        }

        private void ApplyBinding(ISqlExpression expression)
        {
            if (Source is JoinExpression join)
            {
                expression = expression.ReplaceJoinBindings(join, false);
            }
            else if (Source is ProjectionExpression projection)
            {
                expression = expression.CompactExpression(projection);

                if (projection.Source is JoinExpression projectionJoin)
                {
                    expression = expression.ReplaceJoinBindings(projectionJoin, false);
                }
            }

            Predicate = Predicate != null
                ? new BinaryExpression(typeof(bool), BinaryOperator.AndAlso, Predicate, expression)
                : expression;
        }

        #endregion
    }
}