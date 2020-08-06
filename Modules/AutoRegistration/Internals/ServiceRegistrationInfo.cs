namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Diagnostics;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics.Exceptions;
    using SimpleInjector;

    [DebuggerDisplay("{ImplementationType.FullName} - {ServiceType.FullName} - {Lifestyle} - {Attribute}")]
    internal class ServiceRegistrationInfo
    {
        internal ServiceRegistrationInfo(Type serviceType, Type implementationType, EnLifestyle lifestyle)
        {
            ServiceType = serviceType.IsGenericType
                              ? serviceType.GetGenericTypeDefinition()
                              : serviceType;

            ImplementationType = implementationType;

            Lifestyle = lifestyle.MapLifestyle();
        }

        internal Type ImplementationType { get; }

        internal Type ServiceType { get; }

        internal Lifestyle Lifestyle { get; }

        internal Type? Attribute { get; set; }
    }
}