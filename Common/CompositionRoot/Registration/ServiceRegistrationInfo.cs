namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    /// <summary>
    /// ServiceRegistrationInfo
    /// </summary>
    public class ServiceRegistrationInfo : IRegistrationInfo,
                                           IEquatable<ServiceRegistrationInfo>,
                                           ISafelyEquatable<ServiceRegistrationInfo>
    {
        /// <summary> .cctor </summary>
        /// <param name="service">Service</param>
        /// <param name="implementation">Implementation</param>
        /// <param name="lifestyle">EnLifestyle</param>
        public ServiceRegistrationInfo(Type service, Type implementation, EnLifestyle lifestyle)
        {
            Service = service.GenericTypeDefinitionOrSelf();
            Implementation = implementation;
            Lifestyle = lifestyle;
        }

        /// <inheritdoc />
        public Type Service { get; }

        /// <summary>
        /// Implementation
        /// </summary>
        public Type Implementation { get; }

        /// <inheritdoc />
        public EnLifestyle Lifestyle { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left ServiceRegistrationInfo</param>
        /// <param name="right">Right ServiceRegistrationInfo</param>
        /// <returns>equals</returns>
        public static bool operator ==(ServiceRegistrationInfo? left, ServiceRegistrationInfo? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left ServiceRegistrationInfo</param>
        /// <param name="right">Right ServiceRegistrationInfo</param>
        /// <returns>not equals</returns>
        public static bool operator !=(ServiceRegistrationInfo? left, ServiceRegistrationInfo? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public bool SafeEquals(ServiceRegistrationInfo other)
        {
            return Service == other.Service
                    && Implementation == other.Implementation
                    && Lifestyle == other.Lifestyle;
        }

        /// <inheritdoc />
        public bool Equals(ServiceRegistrationInfo? other)
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
            return HashCode.Combine(Service, Implementation, Lifestyle);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return (Service, Implementation, Lifestyle).ToString(" | ");
        }

        internal bool IsOpenGenericFallback()
        {
            return !Implementation.IsConstructedOrNonGenericType();
        }
    }
}