namespace SpaceEngineers.Core.GenericEndpoint.Verifiers
{
    using System;
    using System.Linq;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using AutoWiringApi.Services;
    using Basics;
    using NServiceBus;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class HandleMessagesMustBeRegistered : IConfigurationVerifier
    {
        private readonly ITypeProvider _typeProvider;

        public HandleMessagesMustBeRegistered(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public void Verify()
        {
            _typeProvider
               .OurTypes
               .Where(type => type.IsSubclassOfOpenGeneric(typeof(IHandleMessages<>))
                           && !typeof(IResolvable).IsAssignableFrom(type))
               .Each(type => throw new InvalidOperationException($"{type.FullName} must be registered in dependency container"));
        }
    }
}