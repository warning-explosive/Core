namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Diagnostics;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics.Exceptions;
    using SimpleInjector;

    [DebuggerDisplay("{ComponentType.FullName} - {ServiceType.FullName} - {Lifestyle} - {Attribute}")]
    internal class ServiceRegistrationInfo
    {
        internal ServiceRegistrationInfo(Type serviceType,
                                         Type componentType,
                                         EnLifestyle? lifestyle)
        {
            ServiceType = serviceType.IsGenericType
                              ? serviceType.GetGenericTypeDefinition()
                              : serviceType;
            ComponentType = componentType;

            if (lifestyle == null)
            {
                throw new AttributeRequiredException(typeof(LifestyleAttribute), componentType);
            }

            Lifestyle = lifestyle.Value.MapLifestyle();
        }

        internal Type ComponentType { get; }

        internal Type ServiceType { get; }

        internal Lifestyle Lifestyle { get; }

        internal Type? Attribute { get; set; }
    }
}