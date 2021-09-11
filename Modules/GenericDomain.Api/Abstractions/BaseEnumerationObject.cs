namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Basics;

    /// <summary>
    /// BaseEnumerationObject
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public abstract class BaseEnumerationObject : IEnumerationObject,
                                                  IEquatable<BaseEnumerationObject>,
                                                  ISafelyEquatable<BaseEnumerationObject>
    {
        /// <summary> .cctor </summary>
        /// <param name="id">Identifier</param>
        /// <param name="name">Name</param>
        protected BaseEnumerationObject(int id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <inheritdoc />
        public int Id { get; private set; }

        /// <inheritdoc />
        public string Name { get; private set; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left BaseEnumerationObject</param>
        /// <param name="right">Right BaseEnumerationObject</param>
        /// <returns>equals</returns>
        public static bool operator ==(BaseEnumerationObject? left, BaseEnumerationObject? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left BaseEnumerationObject</param>
        /// <param name="right">Right BaseEnumerationObject</param>
        /// <returns>not equals</returns>
        public static bool operator !=(BaseEnumerationObject? left, BaseEnumerationObject? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, GetType());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(BaseEnumerationObject? other)
        {
            return Equatable.Equals<BaseEnumerationObject>(this, other);
        }

        /// <inheritdoc />
        public bool Equals(IEnumerationObject? other)
        {
            return Equatable.Equals<IEnumerationObject>(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(BaseEnumerationObject other)
        {
            return SafeEquals((IEnumerationObject)other);
        }

        /// <inheritdoc />
        public bool SafeEquals(IEnumerationObject other)
        {
            return Id.Equals(other.Id);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString() => Name;

        /// <summary> All </summary>
        /// <typeparam name="TEnumeration">TEnumeration type-argument</typeparam>
        /// <returns>All enumeration values</returns>
        public static IEnumerable<TEnumeration> All<TEnumeration>()
            where TEnumeration : BaseEnumerationObject
        {
            return typeof(TEnumeration)
                .GetFields(BindingFlags.Public
                           | BindingFlags.Static
                           | BindingFlags.DeclaredOnly)
                .Select(field => field.GetValue(null))
                .OfType<TEnumeration>();
        }
    }
}