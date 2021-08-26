namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Abstractions;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    /// <summary>
    /// ServiceRegistrationInfo
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    [DebuggerDisplay("{Implementation.FullName} - {Service.FullName} - {Lifestyle}")]
    public class ServiceRegistrationInfo : IEquatable<ServiceRegistrationInfo>,
                                           ISafelyEquatable<ServiceRegistrationInfo>,
                                           IRegistrationInfo
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
        /// Implementation type
        /// </summary>
        public Type Implementation { get; }

        /// <inheritdoc />
        public EnLifestyle Lifestyle { get; }

        #region IEquatable

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

        internal bool IsOpenGenericFallback()
        {
            return Implementation.IsGenericType
                   && !Implementation.IsConstructedGenericType;
        }
    }
}