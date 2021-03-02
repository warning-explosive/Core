namespace SpaceEngineers.Core.AutoRegistration.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using AutoWiring.Api.Services;
    using Basics;
    using Implementations;
    using SimpleInjector;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class ManualRegistrationTypesMustBeRegistered : AttributesConfigurationVerifierBase
    {
        private readonly ITypeProvider _typeProvider;

        public ManualRegistrationTypesMustBeRegistered(Container container, ITypeProvider typeProvider)
            : base(container)
        {
            _typeProvider = typeProvider;
        }

        protected override void VerifyInternal(ICollection<Type> registered)
        {
            _typeProvider
               .OurTypes
               .Where(SkipBypassTypes)
               .Where(type => type.HasAttribute<ManualRegistrationAttribute>())
               .SelectMany(implementation => ExtractAutoWiringServices(implementation).Select(service => (service, implementation)))
               .Where(pair => !registered.Contains(pair.implementation))
               .Each(pair => throw new InvalidOperationException($"{pair.implementation.FullName} marked with the {nameof(ManualRegistrationAttribute)}. You must register it yourself as {pair.service.FullName}."));
        }

        private static bool SkipBypassTypes(Type type)
        {
            return type != typeof(Versioned<>);
        }
    }
}