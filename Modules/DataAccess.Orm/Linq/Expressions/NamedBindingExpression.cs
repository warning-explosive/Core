namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Abstractions;
    using Basics;

    /// <summary>
    /// NamedBindingExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class NamedBindingExpression : INamedIntermediateExpression,
                                          IEquatable<NamedBindingExpression>,
                                          ISafelyEquatable<NamedBindingExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="expression">Expression</param>
        /// <param name="name">Alias name</param>
        public NamedBindingExpression(IIntermediateExpression expression, string name)
        {
            Expression = expression;
            Name = name;
        }

        /// <inheritdoc />
        public Type Type => Expression.Type;

        /// <inheritdoc />
        public string Name { get; }

        /// <summary>
        /// Expression
        /// </summary>
        public IIntermediateExpression Expression { get; }

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
            return HashCode.Combine(Type, Name, Expression);
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
                   && Expression.Equals(other.Expression);
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
                ? namedBinding.Expression
                : expression;
        }
    }
}