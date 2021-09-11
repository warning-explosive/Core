namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using Basics;
    using Exceptions;

    /// <summary>
    /// GroupByExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class GroupByExpression : IIntermediateExpression,
                                     IEquatable<GroupByExpression>,
                                     ISafelyEquatable<GroupByExpression>,
                                     IApplicable<ProjectionExpression>,
                                     IApplicable<FilterExpression>,
                                     IApplicable<NamedSourceExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="valuesExpressionProducer">Values expression producer</param>
        public GroupByExpression(
            Type type,
            Func<IReadOnlyDictionary<string, object?>, IIntermediateExpression> valuesExpressionProducer)
        {
            Type = type;
            ValuesExpressionProducer = valuesExpressionProducer;
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Keys expression
        /// </summary>
        public IIntermediateExpression KeysExpression { get; private set; } = null!;

        /// <summary>
        /// Values expression producer
        /// </summary>
        public Func<IReadOnlyDictionary<string, object?>, IIntermediateExpression> ValuesExpressionProducer { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left GroupByExpression</param>
        /// <param name="right">Right GroupByExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(GroupByExpression? left, GroupByExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left GroupByExpression</param>
        /// <param name="right">Right GroupByExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(GroupByExpression? left, GroupByExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, KeysExpression);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(GroupByExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(GroupByExpression other)
        {
            return Type == other.Type
                   && KeysExpression.Equals(other.KeysExpression);
        }

        #endregion

        /// <inheritdoc />
        public Expression AsExpressionTree()
        {
            throw new TranslationException(nameof(GroupByExpression) + "." + nameof(AsExpressionTree));
        }

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, ProjectionExpression expression)
        {
            ApplyInternal(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, FilterExpression expression)
        {
            ApplyInternal(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, NamedSourceExpression expression)
        {
            ApplyInternal(expression);
        }

        private void ApplyInternal(IIntermediateExpression expression)
        {
            KeysExpression ??= expression;
        }

        #endregion
    }
}