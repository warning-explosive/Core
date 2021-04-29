namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Extensions;
    using SimpleInjector;

    internal class DelegateRegistrationInfo : IRegistrationInfo
    {
        internal DelegateRegistrationInfo(
            Type serviceType,
            Func<object> factory,
            EnLifestyle lifestyle)
        {
            ServiceType = serviceType.GenericTypeDefinitionOrSelf();
            Factory = factory;
            Lifestyle = lifestyle.MapLifestyle();
        }

        public Type ServiceType { get; }

        public Lifestyle Lifestyle { get; }

        public Func<object> Factory { get; }
    }
}