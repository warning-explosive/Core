namespace SpaceEngineers.Core.AutoRegistration.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using AutoWiring.Api.Services;
    using Basics;
    using Extensions;
    using SimpleInjector;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class InitializableServicesCannotBeInjected : IConfigurationVerifier
    {
        private readonly ITypeProvider _typeProvider;
        private readonly Container _container;

        public InitializableServicesCannotBeInjected(Container container, ITypeProvider typeProvider)
        {
            _container = container;
            _typeProvider = typeProvider;
        }

        public void Verify()
        {
            var initializableComponents = _typeProvider
                .OurTypes
                .Where(type => type.IsSubclassOfOpenGeneric(typeof(IInitializable<>)))
                .Select(type => type.GenericTypeDefinitionOrSelf())
                .Distinct()
                .ToList();

            _container
                .RegisteredComponents()
                .Where(type => _container
                    .ResolutionConstructor(type)
                    .GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .Select(t => t.UnwrapTypeParameter(typeof(IEnumerable<>)))
                    .Select(t => t.UnwrapTypeParameter(typeof(IVersioned<>)))
                    .Select(t => t.GenericTypeDefinitionOrSelf())
                    .Any(WrongParameter))
                .Each(type => throw new InvalidOperationException($"Component {type.FullName} shouldn't depends on {typeof(IInitializable<>).Name} service"));

            bool WrongParameter(Type type)
            {
                return initializableComponents.Any(cmp => cmp == type);
            }
        }
    }
}