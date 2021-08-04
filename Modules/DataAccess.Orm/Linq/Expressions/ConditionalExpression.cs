namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Abstractions;
    using Basics;

    /// <summary>
    /// ConditionalExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class ConditionalExpression : IIntermediateExpression,
                                         IEquatable<ConditionalExpression>,
                                         ISafelyEquatable<ConditionalExpression>,
                                         IApplicable<IIntermediateExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="when">When IIntermediateExpression</param>
        /// <param name="then">Then IIntermediateExpression</param>
        /// <param name="else">Else IIntermediateExpression</param>
        public ConditionalExpression(
            Type type,
            IIntermediateExpression when,
            IIntermediateExpression then,
            IIntermediateExpression @else)
        {
            Type = type;
            When = when;
            Then = then;
            Else = @else;
        }

        internal ConditionalExpression(Type type)
            : this(type, null !, null !, null !)
        {
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// When condition
        /// </summary>
        public IIntermediateExpression When { get; private set; } = null!;

        /// <summary>
        /// Then expression
        /// </summary>
        public IIntermediateExpression Then { get; private set; } = null!;

        /// <summary>
        /// Then expression
        /// </summary>
        public IIntermediateExpression Else { get; private set; } = null!;

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left ConditionalExpression</param>
        /// <param name="right">Right ConditionalExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(ConditionalExpression? left, ConditionalExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left ConditionalExpression</param>
        /// <param name="right">Right ConditionalExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(ConditionalExpression? left, ConditionalExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, When, Then, Else);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(ConditionalExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(ConditionalExpression other)
        {
            return Type == other.Type
                   && When.Equals(other.When)
                   && Then.Equals(other.Then)
                   && Else.Equals(other.Else);
        }

        #endregion

        /// <inheritdoc />
        public void Apply(TranslationContext context, IIntermediateExpression expression)
        {
            if (When == null)
            {
                When = expression;
            }
            else if (Then == null)
            {
                Then = expression;
            }
            else if (Else == null)
            {
                Else = expression;
            }
        }
    }
}