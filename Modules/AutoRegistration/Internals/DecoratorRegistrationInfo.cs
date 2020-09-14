namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using AutoWiringApi.Enumerations;

    internal class DecoratorRegistrationInfo : ServiceRegistrationInfo
    {
        internal DecoratorRegistrationInfo(Type serviceType, Type implementationType, EnLifestyle lifestyle)
            : base(serviceType, implementationType, lifestyle)
        {
        }

        internal Type? Attribute { get; set; }
    }
}