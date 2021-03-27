namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Diagnostics;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Extensions;
    using SimpleInjector;

    [DebuggerDisplay("{ImplementationType.FullName} - {ServiceType.FullName} - {Lifestyle}")]
    internal class ServiceRegistrationInfo : IEquatable<ServiceRegistrationInfo>
    {
        internal ServiceRegistrationInfo(Type serviceType, Type implementationType)
        {
            ServiceType = serviceType.GenericTypeDefinitionOrSelf();
            ImplementationType = implementationType;

            var implementationComponentAttribute = implementationType.GetRequiredAttribute<ComponentAttribute>();
            var serviceComponentAttribute = serviceType.GetAttribute<ComponentAttribute>();

            Lifestyle = implementationComponentAttribute.Lifestyle.MapLifestyle();
            ComponentKind = serviceComponentAttribute?.Kind == EnComponentKind.Unregistered
                ? EnComponentKind.Unregistered
                : implementationComponentAttribute.Kind;
        }

        internal Type ImplementationType { get; }

        internal Type ServiceType { get; }

        internal Lifestyle Lifestyle { get; }

        internal EnComponentKind ComponentKind { get; }

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
                   && Lifestyle.Equals(other.Lifestyle)
                   && ComponentKind == other.ComponentKind;
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
            return HashCode.Combine(ImplementationType, ServiceType, Lifestyle, ComponentKind);
        }
    }
}