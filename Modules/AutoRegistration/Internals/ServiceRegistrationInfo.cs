namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Diagnostics;
    using AutoWiringApi.Enumerations;
    using Basics;
    using Extensions;
    using SimpleInjector;

    [DebuggerDisplay("{ImplementationType.FullName} - {ServiceType.FullName} - {Lifestyle}")]
    internal class ServiceRegistrationInfo
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
    }
}