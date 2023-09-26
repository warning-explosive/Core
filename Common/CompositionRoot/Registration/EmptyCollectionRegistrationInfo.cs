namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    /// <summary>
    /// EmptyCollectionRegistrationInfo
    /// </summary>
    public class EmptyCollectionRegistrationInfo : IRegistrationInfo,
                                                   IEquatable<EmptyCollectionRegistrationInfo>,
                                                   ISafelyEquatable<EmptyCollectionRegistrationInfo>
    {
        /// <summary> .cctor </summary>
        /// <param name="service">Service</param>
        public EmptyCollectionRegistrationInfo(Type service)
        {
            Service = service;
        }

        /// <inheritdoc />
        public Type Service { get; }

        /// <inheritdoc />
        public EnLifestyle Lifestyle => EnLifestyle.Singleton;

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left EmptyCollectionRegistrationInfo</param>
        /// <param name="right">Right EmptyCollectionRegistrationInfo</param>
        /// <returns>equals</returns>
        public static bool operator ==(EmptyCollectionRegistrationInfo? left, EmptyCollectionRegistrationInfo? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left EmptyCollectionRegistrationInfo</param>
        /// <param name="right">Right EmptyCollectionRegistrationInfo</param>
        /// <returns>not equals</returns>
        public static bool operator !=(EmptyCollectionRegistrationInfo? left, EmptyCollectionRegistrationInfo? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public bool SafeEquals(EmptyCollectionRegistrationInfo other)
        {
            return Service == other.Service;
        }

        /// <inheritdoc />
        public bool Equals(EmptyCollectionRegistrationInfo? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Service);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(" | ", Service);
        }
    }
}