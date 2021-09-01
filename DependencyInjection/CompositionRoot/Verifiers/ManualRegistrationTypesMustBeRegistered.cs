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
        private readonly Abstractions.IComponentsOverrideContainer _overrides;
        private readonly ITypeProvider _typeProvider;

        public ManualRegistrationTypesMustBeRegistered(
            IRegistrationsContainer registrations,
            Abstractions.IComponentsOverrideContainer overrides,
            ITypeProvider typeProvider)
            : base(registrations)
        {
            _overrides = overrides;
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
               .SelectMany(info => UnwrapAutoRegistrationServices(info.implementation)
                   .Select(service => (service, info.implementation, info.attribute)))
               .Where(info => !registered.Contains(info.implementation) && !HasBeenOverridden(info.service, info.implementation))
               .Each(info => throw new InvalidOperationException($"{info.implementation.FullName} should be manually registered in the dependency container as {info.service.FullName}. Justification: {info.attribute.Justification}"));
        }

        private bool HasBeenOverridden(Type service, Type implementation)
        {
            return _overrides.InstanceOverrides
                .Select(info => new { info.Service, Implementation = info.Instance.GetType() })
                .Concat(_overrides.ResolvableOverrides
                    .Concat(_overrides.CollectionResolvableOverrides)
                    .Concat(_overrides.DecoratorOverrides)
                    .Select(info => new { info.Service, info.Implementation }))
                .Any(info => info.Service == service && info.Implementation == implementation);
        }
    }
}