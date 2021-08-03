namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using Abstractions;
    using Basics;
    using Internals;

    /// <summary>
    /// FilterExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class FilterExpression : ISubsequentIntermediateExpression,
                                    IEquatable<FilterExpression>,
                                    ISafelyEquatable<FilterExpression>,
                                    IApplicable<ProjectionExpression>,
                                    IApplicable<QuerySourceExpression>,
                                    IApplicable<ParameterExpression>,
                                    IApplicable<BinaryExpression>,
                                    IApplicable<ConditionalExpression>,
                                    IApplicable<SimpleBindingExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">Item type</param>
        /// <param name="source">Source</param>
        /// <param name="expression">Expression</param>
        public FilterExpression(
            Type itemType,
            IIntermediateExpression source,
            IIntermediateExpression expression)
        {
            ItemType = itemType;
            Source = source;
            Expression = expression;
        }

        internal FilterExpression(Type itemType)
        {
            ItemType = itemType;
        }

        /// <inheritdoc />
        public Type ItemType { get; }

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
            return HashCode.Combine(ItemType, Source, Expression);
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
            return ItemType == other.ItemType
                   && Source.Equals(other.Source)
                   && Expression.Equals(other.Expression);
        }

        #endregion

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
        public void Apply(TranslationContext context, ParameterExpression parameter)
        {
            Expression = new ReplaceParameterVisitor(parameter).Visit(Expression);

            if (Source is ProjectionExpression projection)
            {
                if (projection.IsProjectionToClass)
                {
                    projection.Apply(context, parameter);
                }
                else
                {
                    var binding = projection.Bindings.Single();
                    Expression = new ReplaceParameterVisitor(NamedBindingExpression.Unwrap(binding)).Visit(Expression);
                }
            }
            else
            {
                Source = new NamedSourceExpression(
                    Source.ItemType,
                    Source,
                    parameter);
            }
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, BinaryExpression binary)
        {
            ApplyExpression(binary);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ConditionalExpression conditional)
        {
            ApplyExpression(conditional);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, SimpleBindingExpression binding)
        {
            ApplyExpression(binding);
        }

        private void ApplyExpression(IIntermediateExpression expression)
        {
            if (Source is ProjectionExpression { IsProjectionToClass: true } projection)
            {
                expression = new ReplaceFilterExpressionVisitor(projection).Visit(expression);
            }

            Expression = Expression != null
                ? new BinaryExpression(typeof(bool), ExpressionType.AndAlso, Expression, expression)
                : expression;
        }
    }
}