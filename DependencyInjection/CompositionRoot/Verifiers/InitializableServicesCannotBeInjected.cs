namespace SpaceEngineers.Core.CompositionRoot.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Api.Abstractions;
    using Api.Abstractions.Registration;
    using Api.Extensions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Extensions;

    [Component(EnLifestyle.Singleton)]
    internal class InitializableServicesCannotBeInjected : IConfigurationVerifier,
                                                           ICollectionResolvable<IConfigurationVerifier>
    {
        private readonly ITypeProvider _typeProvider;
        private readonly IRegistrationsContainer _registrations;
        private readonly DependencyContainerOptions _options;

        public InitializableServicesCannotBeInjected(
            IRegistrationsContainer registrations,
            ITypeProvider typeProvider,
            DependencyContainerOptions options)
        {
            _registrations = registrations;
            _typeProvider = typeProvider;
            _options = options;
        }

        public void Verify()
        {
            var initializableComponents = _typeProvider
                .OurTypes
                .Where(type => type.IsSubclassOfOpenGeneric(typeof(IInitializable<>)))
                .Select(type => type.GenericTypeDefinitionOrSelf())
                .Distinct()
                .ToList();

            _registrations
                .RegisteredComponents()
                .Where(HasWrongConstructor(initializableComponents))
                .Each(type => throw new InvalidOperationException($"Component {type.FullName} shouldn't depends on {typeof(IInitializable<>).Name} service"));
        }

        private Func<Type, bool> HasWrongConstructor(IReadOnlyCollection<Type> initializableComponents)
        {
            return type =>
            {
                var cctor = _options
                    .ConstructorResolutionBehavior
                    .GetWiringConstructor(type);

                return cctor
                    .GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .Select(t => t.UnwrapTypeParameter(typeof(IEnumerable<>)))
                    .Select(t => t.GenericTypeDefinitionOrSelf())
                    .Any(parameter => WrongParameter(type, cctor, parameter, initializableComponents));
            };
        }

        private static bool WrongParameter(
            Type declaringType,
            ConstructorInfo constructorInfo,
            Type parameterType,
            IEnumerable<Type> initializableComponents)
        {
            var isImplementation = parameterType.IsAssignableFrom(declaringType)
                                   || declaringType.IsSubclassOfOpenGeneric(parameterType.GenericTypeDefinitionOrSelf());

            if (isImplementation
                && constructorInfo.IsDecorator(parameterType))
            {
                return false;
            }

            return initializableComponents.Any(cmp => cmp == parameterType);
        }
    }
}