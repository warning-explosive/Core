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
        /// <param name="name">Binding name</param>
        /// <param name="source">Source expression</param>
        public NamedBindingExpression(string name, IIntermediateExpression source)
        {
            Name = name;
            Source = source;
        }

        internal NamedBindingExpression(string name)
            : this(name, null!)
        {
        }

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
            return HashCode.Combine(
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
        public bool Equals(NamedBindingExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(NamedBindingExpression other)
        {
            return Type == other.Type
                   && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase)
                   && Source.Equals(other.Source);
        }

        #endregion

        /// <inheritdoc />
        public Expression AsExpressionTree()
        {
            throw new TranslationException(nameof(NamedBindingExpression) + "." + nameof(AsExpressionTree));
        }

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

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, SimpleBindingExpression expression)
        {
            ApplySource(expression);
        }

        private void ApplySource(SimpleBindingExpression expression)
        {
            if (Source != null)
            {
                throw new InvalidOperationException("Named binding expression source has already been set");
            }

            Source = expression;
        }

        #endregion
    }
}