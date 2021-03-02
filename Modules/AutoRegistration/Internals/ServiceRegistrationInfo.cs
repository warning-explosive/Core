namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Diagnostics;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Extensions;
    using SimpleInjector;

    [DebuggerDisplay("{ImplementationType.FullName} - {ServiceType.FullName} - {Lifestyle}")]
    internal class ServiceRegistrationInfo : IEquatable<ServiceRegistrationInfo>
    {
        internal ServiceRegistrationInfo(Type serviceType, Type implementationType, EnLifestyle lifestyle)
        {
            ServiceType = serviceType.GenericTypeDefinitionOrSelf();
            ImplementationType = implementationType;
            Lifestyle = lifestyle.MapLifestyle();
        }

        internal Type ImplementationType { get; }

        internal Type ServiceType { get; }

        internal Lifestyle Lifestyle { get; }

        public bool Equals(ServiceRegistrationInfo? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return ServiceType == other.ServiceType
                   && ImplementationType == other.ImplementationType
                   && Lifestyle.Equals(other.Lifestyle);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType()
                   && Equals((ServiceRegistrationInfo)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ImplementationType, ServiceType, Lifestyle);
        }
    }
}