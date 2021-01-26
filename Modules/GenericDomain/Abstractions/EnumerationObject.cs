namespace SpaceEngineers.Core.GenericDomain.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Enumeration object
    /// </summary>
    public abstract class EnumerationObject : IEquatable<EnumerationObject>
    {
        /// <summary> .cctor </summary>
        /// <param name="id">Identifier</param>
        /// <param name="name">Name</param>
        protected EnumerationObject(int id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Identifier
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left EnumerationObject</param>
        /// <param name="right">Right EnumerationObject</param>
        /// <returns>equals</returns>
        public static bool operator ==(EnumerationObject left, EnumerationObject right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left EnumerationObject</param>
        /// <param name="right">Right EnumerationObject</param>
        /// <returns>not equals</returns>
        public static bool operator !=(EnumerationObject left, EnumerationObject right)
        {
            return !left.Equals(right);
        }

        /// <summary> All </summary>
        /// <typeparam name="TEnumeration">TEnumeration type-argument</typeparam>
        /// <returns>All enumeration values</returns>
        public static IEnumerable<TEnumeration> All<TEnumeration>()
            where TEnumeration : EnumerationObject
        {
            return typeof(TEnumeration)
                .GetFields(BindingFlags.Public
                           | BindingFlags.Static
                           | BindingFlags.DeclaredOnly)
                .Select(f => f.GetValue(null))
                .OfType<TEnumeration>();
        }

        /// <inheritdoc />
        public override string ToString() => Name;

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, GetType());
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is EnumerationObject otherValue
                   && Equals(otherValue);
        }

        /// <inheritdoc />
        public bool Equals(EnumerationObject? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var typeMatches = GetType() == other.GetType();
            var valueMatches = Id.Equals(other.Id);

            return typeMatches && valueMatches;
        }
    }
}