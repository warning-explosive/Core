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
    internal class UnregisteredTypesMustBeUnregistered : AttributesConfigurationVerifierBase,
                                                         ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly ITypeProvider _typeProvider;

        public UnregisteredTypesMustBeUnregistered(
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
               .Where(type => type.HasAttribute<UnregisteredComponentAttribute>())
               .SelectMany(implementation => ExtractAutoWiringServices(implementation).Select(service => (service, implementation)))
               .Where(pair => registered.Contains(pair.implementation) || registered.Contains(pair.service))
               .Each(pair => throw new InvalidOperationException($"{pair.implementation.FullName} shouldn't be registered but represented in the dependency container as {pair.service.FullName}"));
        }
    }
}