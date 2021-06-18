namespace SpaceEngineers.Core.AutoRegistration.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using SimpleInjector;

    [Component(EnLifestyle.Singleton)]
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
               .Select(type => (type, type.GetAttribute<ComponentAttribute>()?.RegistrationKind))
               .Where(info => info.RegistrationKind == EnComponentRegistrationKind.ManuallyRegistered)
               .Select(info => info.type)
               .SelectMany(implementation => ExtractAutoWiringServices(implementation).Select(service => (service, implementation)))
               .Where(pair => !registered.Contains(pair.implementation))
               .Each(pair => throw new InvalidOperationException($"{pair.implementation.FullName} should be registered manually in the dependency container as {pair.service.FullName}"));
        }
    }
}