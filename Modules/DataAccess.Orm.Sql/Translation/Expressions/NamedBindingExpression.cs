namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;
    using Api.Exceptions;
    using Basics;

    /// <summary>
    /// NamedBindingExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class NamedBindingExpression : IBindingIntermediateExpression,
                                          IEquatable<NamedBindingExpression>,
                                          ISafelyEquatable<NamedBindingExpression>,
                                          IApplicable<SimpleBindingExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="member">Member info</param>
        /// <param name="expression">Expression</param>
        public NamedBindingExpression(MemberInfo member, IIntermediateExpression expression)
        {
            Member = member;
            Source = expression;
            Name = member.Name;
        }

        internal NamedBindingExpression(MemberInfo member)
            : this(member, null !)
        {
        }

        /// <inheritdoc />
        public MemberInfo Member { get; }

        /// <inheritdoc />
        public Type Type => Source.Type;

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public IIntermediateExpression Source { get; private set; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left NamedBindingExpression</param>
        /// <param name="right">Right NamedBindingExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(NamedBindingExpression? left, NamedBindingExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left NamedBindingExpression</param>
        /// <param name="right">Right NamedBindingExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(NamedBindingExpression? left, NamedBindingExpression? right)
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
        public bool Equals(NamedBindingExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(NamedBindingExpression other)
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
            throw new TranslationException(nameof(NamedBindingExpression) + "." + nameof(AsExpressionTree));
        }

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, SimpleBindingExpression expression)
        {
            Source = expression;
        }

        #endregion

        /// <summary>
        /// Unwrap NamedBindingExpression
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <returns>Unwrapped expression</returns>
        public static IIntermediateExpression Unwrap(IIntermediateExpression expression)
        {
            return expression is NamedBindingExpression namedBinding
                ? namedBinding.Source
                : expression;
        }
    }
}