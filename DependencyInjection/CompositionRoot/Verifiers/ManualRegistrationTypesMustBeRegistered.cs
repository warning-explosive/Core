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
               .Select(type =>
               {
                   var implementation = type;
                   var attribute = type.GetAttribute<ManuallyRegisteredComponentAttribute>();
                   return (implementation, attribute);
               })
               .Where(info => info.attribute != null)
               .SelectMany(info => ExtractAutoWiringServices(info.implementation)
                   .Select(service => (service, info.implementation, info.attribute)))
               .Where(info => !registered.Contains(info.implementation))
               .Each(info => throw new InvalidOperationException($"{info.implementation.FullName} should be manually registered in the dependency container as {info.service.FullName}. Justification: {info.attribute.Justification}"));
        }
    }
}