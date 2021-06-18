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
    internal class UnregisteredTypesMustBeUnregistered : AttributesConfigurationVerifierBase
    {
        private readonly ITypeProvider _typeProvider;

        public UnregisteredTypesMustBeUnregistered(Container container, ITypeProvider typeProvider)
            : base(container)
        {
            _typeProvider = typeProvider;
        }

        protected override void VerifyInternal(ICollection<Type> registered)
        {
            _typeProvider
               .OurTypes
               .Select(type => (type, type.GetAttribute<ComponentAttribute>()?.RegistrationKind))
               .Where(info => info.RegistrationKind == EnComponentRegistrationKind.Unregistered)
               .Select(info => info.type)
               .SelectMany(implementation => ExtractAutoWiringServices(implementation).Select(service => (service, implementation)))
               .Where(pair => registered.Contains(pair.implementation) || registered.Contains(pair.service))
               .Each(pair => throw new InvalidOperationException($"{pair.implementation.FullName} shouldn't be registered but represented in the dependency container as {pair.service.FullName}"));
        }
    }
}