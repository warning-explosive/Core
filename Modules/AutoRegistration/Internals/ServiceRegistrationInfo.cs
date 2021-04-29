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
    internal class ServiceRegistrationInfo : IEquatable<ServiceRegistrationInfo>,
                                             ISafelyEquatable<ServiceRegistrationInfo>,
                                             IRegistrationInfo
    {
        internal ServiceRegistrationInfo(Type serviceType, Type implementationType)
        {
            ServiceType = serviceType.GenericTypeDefinitionOrSelf();
            ImplementationType = implementationType;

            var implementationComponentAttribute = implementationType.GetRequiredAttribute<ComponentAttribute>();
            Lifestyle = implementationComponentAttribute.Lifestyle.MapLifestyle();

            RegistrationKind = serviceType.GetAttribute<ComponentAttribute>()?.RegistrationKind == EnComponentRegistrationKind.Unregistered
                ? EnComponentRegistrationKind.Unregistered
                : implementationComponentAttribute.RegistrationKind;
        }

        public Type ServiceType { get; }

        public Lifestyle Lifestyle { get; }

        public Type ImplementationType { get; }

        public EnComponentRegistrationKind RegistrationKind { get; }

        public bool SafeEquals(ServiceRegistrationInfo other)
        {
            return ServiceType == other.ServiceType
                    && ImplementationType == other.ImplementationType
                    && Lifestyle.Equals(other.Lifestyle)
                    && RegistrationKind == other.RegistrationKind;
        }

        public bool Equals(ServiceRegistrationInfo? other)
        {
            return Equatable.Equals(this, other);
        }

        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ImplementationType, ServiceType, Lifestyle, RegistrationKind);
        }
    }
}