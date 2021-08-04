namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Abstractions;
    using Basics;

    /// <summary>
    /// ParameterExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class ParameterExpression : IIntermediateExpression,
                                       IEquatable<ParameterExpression>,
                                       ISafelyEquatable<ParameterExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        public ParameterExpression(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left ParameterExpression</param>
        /// <param name="right">Right ParameterExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(ParameterExpression? left, ParameterExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left ParameterExpression</param>
        /// <param name="right">Right ParameterExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(ParameterExpression? left, ParameterExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Name);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(ParameterExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(ParameterExpression other)
        {
            return Type == other.Type
                   && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}