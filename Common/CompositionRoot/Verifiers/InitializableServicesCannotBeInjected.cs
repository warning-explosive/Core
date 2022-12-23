namespace SpaceEngineers.Core.CompositionRoot.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Extensions;
    using Registration;

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
            var exceptions = new List<Exception>();

            var initializableComponents = _typeProvider
                .OurTypes
                .Where(type => type.IsSubclassOfOpenGeneric(typeof(IInitializable<>)))
                .Select(type => type.GenericTypeDefinitionOrSelf())
                .Distinct()
                .ToList();

            VerifyConstructors(initializableComponents, exceptions);

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        private void VerifyConstructors(
            IReadOnlyCollection<Type> initializableComponents,
            ICollection<Exception> exceptions)
        {
            var types = _registrations
                .RegisteredComponents()
                .Where(HasWrongConstructor(initializableComponents, _options.ConstructorResolutionBehavior));

            foreach (var type in types)
            {
                exceptions.Add(new InvalidOperationException($"Component {type.FullName} shouldn't depends on {typeof(IInitializable<>).Name} service"));
            }

            static Func<Type, bool> HasWrongConstructor(
                IReadOnlyCollection<Type> initializableComponents,
                IConstructorResolutionBehavior constructorResolutionBehavior)
            {
                return type =>
                {
                    var cctor = constructorResolutionBehavior.GetAutoWiringConstructor(type);

                    return cctor
                        .GetParameters()
                        .Select(parameter => parameter.ParameterType)
                        .Select(t => t.ExtractGenericArgumentAtOrSelf(typeof(IEnumerable<>)))
                        .Select(t => t.GenericTypeDefinitionOrSelf())
                        .Any(parameter => WrongParameter(type, cctor, parameter, initializableComponents));
                };
            }

            static bool WrongParameter(
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
}