namespace SpaceEngineers.Core.DataAccess.Orm.ValueObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Abstractions;
    using Basics;

    /// <summary>
    /// NamedSourceExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class NamedSourceExpression : ISubsequentIntermediateExpression,
                                         IEquatable<NamedSourceExpression>,
                                         ISafelyEquatable<NamedSourceExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">Item type</param>
        /// <param name="source">Source</param>
        /// <param name="parameter">Parameter</param>
        public NamedSourceExpression(
            Type itemType,
            IIntermediateExpression source,
            ParameterExpression parameter)
        {
            ItemType = itemType;
            Source = source;
            Parameter = parameter;
        }

        /// <inheritdoc />
        public Type ItemType { get; }

        /// <inheritdoc />
        public IIntermediateExpression Source { get; }

        /// <summary>
        /// Parameter expression
        /// </summary>
        public ParameterExpression Parameter { get; }

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
            return HashCode.Combine(ItemType, Source, Parameter);
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
            return ItemType == other.ItemType
                   && Source.Equals(other.Source)
                   && Parameter.Equals(other.Parameter);
        }

        #endregion
    }
}