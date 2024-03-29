namespace SpaceEngineers.Core.CompositionRoot.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Registration;

    [Component(EnLifestyle.Singleton)]
    internal class ManualRegistrationTypesMustBeRegistered : AttributesConfigurationVerifierBase,
                                                             ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly IComponentsOverrideContainer _overrides;
        private readonly ITypeProvider _typeProvider;

        public ManualRegistrationTypesMustBeRegistered(
            IRegistrationsContainer registrations,
            IComponentsOverrideContainer overrides,
            ITypeProvider typeProvider)
            : base(registrations)
        {
            _overrides = overrides;
            _typeProvider = typeProvider;
        }

        protected override void VerifyInternal(IReadOnlyCollection<Type> registeredComponents)
        {
            var exceptions = new List<Exception>();

            var infos = _typeProvider
               .OurTypes
               .Select(type =>
               {
                   var implementation = type;
                   var attribute = type.GetAttribute<ManuallyRegisteredComponentAttribute>();
                   return (implementation, attribute);
               })
               .Where(info => info.attribute != null)
               .SelectMany(info => GetAutoRegistrationServices(info.implementation)
                   .Select(service => (service, info.implementation, info.attribute)))
               .Where(info => !Registered(registeredComponents, info));

            foreach (var info in infos)
            {
                exceptions.Add(new InvalidOperationException($"{info.implementation.FullName} should be manually registered in the dependency container as {info.service.FullName}. Justification: {info.attribute.Justification}"));
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        private bool Registered(
            IReadOnlyCollection<Type> registeredComponents,
            (Type service, Type implementation, ManuallyRegisteredComponentAttribute? attribute) info)
        {
            return registeredComponents.Contains(info.implementation)
                || HasBeenOverridden(info.service);
        }

        private bool HasBeenOverridden(Type service)
        {
            return _overrides.ResolvableOverrides.Select(info => info.Service)
                .Concat(_overrides.CollectionOverrides.Select(collection => collection.Key))
                .Concat(_overrides.DecoratorOverrides.Select(info => info.Service))
                .Any(overriddenService => overriddenService == service);
        }
    }
}