namespace SpaceEngineers.Core.CompositionRoot.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Singleton)]
    internal class ManualRegistrationTypesMustBeRegistered : AttributesConfigurationVerifierBase,
                                                             ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly ITypeProvider _typeProvider;

        public ManualRegistrationTypesMustBeRegistered(
            IRegistrationsContainer registrations,
            ITypeProvider typeProvider)
            : base(registrations)
        {
            _typeProvider = typeProvider;
        }

        protected override void VerifyInternal(ICollection<Type> registered)
        {
            _typeProvider
               .OurTypes
               .Where(type => type.HasAttribute<ManuallyRegisteredComponentAttribute>())
               .Select(info => info)
               .SelectMany(implementation => ExtractAutoWiringServices(implementation).Select(service => (service, implementation)))
               .Where(pair => !registered.Contains(pair.implementation))
               .Each(pair => throw new InvalidOperationException($"{pair.implementation.FullName} should be manually registered in the dependency container as {pair.service.FullName}"));
        }
    }
}