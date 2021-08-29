namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    /// <summary>
    /// ServiceRegistrationInfo
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    [DebuggerDisplay("{Implementation.FullName} - {Service.FullName} - {Lifestyle}")]
    public class ServiceRegistrationInfo : IEquatable<ServiceRegistrationInfo>,
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

        /// <summary>
        /// Service
        /// </summary>
        public Type Service { get; }

        /// <summary>
        /// Implementation
        /// </summary>
        public Type Implementation { get; }

        /// <summary>
        /// Lifestyle
        /// </summary>
        public EnLifestyle Lifestyle { get; }

        /// <summary>
        /// Override comparer
        /// </summary>
        public static IEqualityComparer<ServiceRegistrationInfo> OverrideComparer { get; } = new ServiceRegistrationInfoComparer();

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(" | ", Service, Implementation, Lifestyle);
        }

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

        private class ServiceRegistrationInfoComparer : IEqualityComparer<ServiceRegistrationInfo>
        {
            public bool Equals(ServiceRegistrationInfo actual, ServiceRegistrationInfo @override)
            {
                if (ReferenceEquals(actual, @override))
                {
                    return true;
                }

                return actual.Service == @override.Service
                       && actual.Implementation == @override.Implementation
                       && actual.Lifestyle <= @override.Lifestyle;
            }

            public int GetHashCode(ServiceRegistrationInfo info)
            {
                return HashCode.Combine(info.Service, info.Implementation, info.Lifestyle);
            }
        }
    }
}