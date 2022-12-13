namespace SpaceEngineers.Core.Dynamic
{
    using System;
    using Basics;

    /// <summary>
    /// DynamicProperty
    /// </summary>
    public sealed class DynamicProperty : IEquatable<DynamicProperty>,
                                          ISafelyEquatable<DynamicProperty>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        public DynamicProperty(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left DynamicProperty</param>
        /// <param name="right">Right DynamicProperty</param>
        /// <returns>equals</returns>
        public static bool operator ==(DynamicProperty? left, DynamicProperty? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left DynamicProperty</param>
        /// <param name="right">Right DynamicProperty</param>
        /// <returns>not equals</returns>
        public static bool operator !=(DynamicProperty? left, DynamicProperty? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Name.ToUpperInvariant());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(DynamicProperty? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(DynamicProperty other)
        {
            return Type == other.Type
                   && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}