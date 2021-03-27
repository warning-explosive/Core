namespace SpaceEngineers.Core.AutoRegistration.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using AutoWiring.Api.Services;
    using Basics;
    using Extensions;
    using SimpleInjector;

    [Component(EnLifestyle.Singleton)]
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
                .Where(WrongConstructor)
                .Each(type => throw new InvalidOperationException($"Component {type.FullName} shouldn't depends on {typeof(IInitializable<>).Name} service"));

            bool WrongConstructor(Type type)
            {
                var cctor = _container.ResolutionConstructor(type);

                return cctor
                    .GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .Select(t => t.UnwrapTypeParameter(typeof(IEnumerable<>)))
                    .Select(t => t.GenericTypeDefinitionOrSelf())
                    .Any(parameter => WrongParameter(type, cctor, parameter));
            }

            bool WrongParameter(Type declaringType, ConstructorInfo constructorInfo, Type parameterType)
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