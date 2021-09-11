namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
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
                                           IApplicable<ParameterExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="member">Member info</param>
        /// <param name="type">Type</param>
        /// <param name="expression">IIntermediateExpression</param>
        public SimpleBindingExpression(
            MemberInfo member,
            Type type,
            IIntermediateExpression expression)
        {
            Member = member;
            Type = type;
            Name = member.Name;
            Source = expression;
        }

        internal SimpleBindingExpression(MemberInfo member, Type type)
            : this(member, type, null !)
        {
        }

        /// <inheritdoc />
        public MemberInfo Member { get; }

        /// <inheritdoc />
        public Type Type { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public IIntermediateExpression Source { get; private set; } = null!;

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
            return HashCode.Combine(Member, Type, Name, Source);
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

        /// <inheritdoc />
        public void Apply(TranslationContext context, SimpleBindingExpression expression)
        {
            Source = expression;
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ParameterExpression expression)
        {
            Source = expression;
        }
    }
}