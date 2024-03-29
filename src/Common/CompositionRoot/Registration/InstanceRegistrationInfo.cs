namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    /// <summary>
    /// InstanceRegistrationInfo
    /// </summary>
    public class InstanceRegistrationInfo : IRegistrationInfo,
                                            IEquatable<InstanceRegistrationInfo>,
                                            ISafelyEquatable<InstanceRegistrationInfo>
    {
        /// <summary> .cctor </summary>
        /// <param name="service">Service</param>
        /// <param name="instance">Instance</param>
        public InstanceRegistrationInfo(Type service, object instance)
        {
            Service = service;
            Instance = instance;
        }

        /// <inheritdoc />
        public Type Service { get; }

        /// <summary>
        /// Instance
        /// </summary>
        public object Instance { get; }

        /// <inheritdoc />
        public EnLifestyle Lifestyle => EnLifestyle.Singleton;

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left InstanceRegistrationInfo</param>
        /// <param name="right">Right InstanceRegistrationInfo</param>
        /// <returns>equals</returns>
        public static bool operator ==(InstanceRegistrationInfo? left, InstanceRegistrationInfo? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left InstanceRegistrationInfo</param>
        /// <param name="right">Right InstanceRegistrationInfo</param>
        /// <returns>not equals</returns>
        public static bool operator !=(InstanceRegistrationInfo? left, InstanceRegistrationInfo? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public bool SafeEquals(InstanceRegistrationInfo other)
        {
            return Service == other.Service
                   && Instance == other.Instance;
        }

        /// <inheritdoc />
        public bool Equals(InstanceRegistrationInfo? other)
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
            return HashCode.Combine(Service, Instance);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return (Service, Instance.GetType(), Lifestyle).ToString(" | ");
        }
    }
}