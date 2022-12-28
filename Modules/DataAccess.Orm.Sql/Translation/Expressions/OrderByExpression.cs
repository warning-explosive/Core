namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Api.Exceptions;
    using Basics;
    using Extensions;

    /// <summary>
    /// OrderByExpression
    /// </summary>
    public class OrderByExpression : ISqlExpression,
                                     IEquatable<OrderByExpression>,
                                     ISafelyEquatable<OrderByExpression>,
                                     IApplicable<OrderByExpression>,
                                     IApplicable<NamedSourceExpression>,
                                     IApplicable<QuerySourceExpression>,
                                     IApplicable<FilterExpression>,
                                     IApplicable<ProjectionExpression>,
                                     IApplicable<JoinExpression>,
                                     IApplicable<OrderByBindingExpression>
    {
        private readonly List<ISqlExpression> _bindings;

        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="source">Source expression</param>
        /// <param name="bindings">Order by expression bindings</param>
        public OrderByExpression(
            Type type,
            ISqlExpression source,
            IReadOnlyCollection<ISqlExpression> bindings)
        {
            Type = type;
            Source = source;
            _bindings = bindings.ToList();
        }

        internal OrderByExpression(Type type)
            : this(type, null!, Array.Empty<ISqlExpression>())
        {
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Source expression
        /// </summary>
        public ISqlExpression Source { get; private set; }

        /// <summary>
        /// Order by expression bindings
        /// </summary>
        public IReadOnlyCollection<ISqlExpression> Bindings => _bindings;

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left OrderByExpression</param>
        /// <param name="right">Right OrderByExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(OrderByExpression? left, OrderByExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left OrderByExpression</param>
        /// <param name="right">Right OrderByExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(OrderByExpression? left, OrderByExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Source, Bindings);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(OrderByExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(OrderByExpression other)
        {
            return Type == other.Type
                && Source.Equals(other.Source)
                && Bindings.SequenceEqual(other.Bindings);
        }

        #endregion

        /// <inheritdoc />
        public Expression AsExpressionTree()
        {
            throw new TranslationException(nameof(OrderByExpression) + "." + nameof(AsExpressionTree));
        }

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, OrderByExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, NamedSourceExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, QuerySourceExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, FilterExpression expression)
        {
            ApplySource(expression);
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
        public void Apply(TranslationContext context, OrderByBindingExpression expression)
        {
            ApplyBinding(expression);
        }

        private void ApplySource(ISqlExpression expression)
        {
            if (Source != null)
            {
                throw new InvalidOperationException("Order by expression source has already been set");
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

            _bindings.Add(expression);
        }

        #endregion
    }
}