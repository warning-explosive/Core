namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;
    using Basics;

    /// <summary>
    /// SimpleBindingExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class SimpleBindingExpression : IBindingIntermediateExpression,
                                           IEquatable<SimpleBindingExpression>,
                                           ISafelyEquatable<SimpleBindingExpression>,
                                           IApplicable<SimpleBindingExpression>,
                                           IApplicable<ParameterExpression>,
                                           IApplicable<QueryParameterExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="member">Member info</param>
        /// <param name="type">Type</param>
        /// <param name="source">Source</param>
        public SimpleBindingExpression(
            MemberInfo member,
            Type type,
            IIntermediateExpression source)
        {
            Member = member;
            Type = type;
            Name = member.Name;
            Source = source;
        }

        internal SimpleBindingExpression(MemberInfo member, Type type)
            : this(member, type, null!)
        {
        }

        /// <summary>
        /// Member
        /// </summary>
        public MemberInfo Member { get; }

        /// <inheritdoc />
        public Type Type { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public IIntermediateExpression Source { get; private set; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left SimpleBindingExpression</param>
        /// <param name="right">Right SimpleBindingExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(SimpleBindingExpression? left, SimpleBindingExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left SimpleBindingExpression</param>
        /// <param name="right">Right SimpleBindingExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(SimpleBindingExpression? left, SimpleBindingExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Member,
                Type,
                Name.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Source);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(SimpleBindingExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(SimpleBindingExpression other)
        {
            return Member == other.Member
                   && Type == other.Type
                   && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase)
                   && Source.Equals(other.Source);
        }

        #endregion

        /// <inheritdoc />
        public Expression AsExpressionTree()
        {
            return System.Linq.Expressions.Expression.MakeMemberAccess(Source.AsExpressionTree(), Member);
        }

        /// <summary>
        /// Gets flat collection of underneath expressions
        /// </summary>
        /// <returns>Flat collection</returns>
        public IEnumerable<IIntermediateExpression> Flatten()
        {
            IIntermediateExpression? current = this;

            while (current != null)
            {
                yield return current;

                current = current is SimpleBindingExpression simpleBindingExpression
                    ? simpleBindingExpression.Source
                    : null;
            }
        }

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, SimpleBindingExpression expression)
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

        private void ApplySource(IIntermediateExpression expression)
        {
            if (Source != null)
            {
                throw new InvalidOperationException("Simple binding expression source has already been set");
            }

            Source = expression;
        }

        #endregion
    }
}