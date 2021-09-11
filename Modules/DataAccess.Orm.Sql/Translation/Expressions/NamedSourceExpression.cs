namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using Basics;
    using Exceptions;

    /// <summary>
    /// NamedSourceExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class NamedSourceExpression : ISubsequentIntermediateExpression,
                                         IEquatable<NamedSourceExpression>,
                                         ISafelyEquatable<NamedSourceExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="source">Source</param>
        /// <param name="parameter">Parameter</param>
        public NamedSourceExpression(
            Type type,
            IIntermediateExpression source,
            IIntermediateExpression parameter)
        {
            Type = type;
            Source = source;
            Parameter = parameter;
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <inheritdoc />
        public IIntermediateExpression Source { get; }

        /// <summary>
        /// Parameter expression
        /// </summary>
        public IIntermediateExpression Parameter { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left NamedSourceExpression</param>
        /// <param name="right">Right NamedSourceExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(NamedSourceExpression? left, NamedSourceExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left NamedSourceExpression</param>
        /// <param name="right">Right NamedSourceExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(NamedSourceExpression? left, NamedSourceExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Source, Parameter);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(NamedSourceExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(NamedSourceExpression other)
        {
            return Type == other.Type
                   && Source.Equals(other.Source)
                   && Parameter.Equals(other.Parameter);
        }

        #endregion

        /// <inheritdoc />
        public Expression AsExpressionTree()
        {
            throw new TranslationException(nameof(NamedSourceExpression) + "." + nameof(AsExpressionTree));
        }
    }
}