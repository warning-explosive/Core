namespace SpaceEngineers.Core.CompositionRoot.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [SuppressMessage("Analysis", "CR1", Justification = "Registered by hand. See DependencyContainerImpl.")]
    [Component(EnLifestyle.Singleton)]
    internal class AutoRegistrationServicesProvider : IAutoRegistrationServicesProvider
    {
        private readonly ITypeProvider _typeProvider;

        public AutoRegistrationServicesProvider(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public IEnumerable<Type> Resolvable()
        {
            return _typeProvider
                  .OurTypes
                  .Where(type => typeof(IResolvable).IsAssignableFrom(type)
                              && type != typeof(IResolvable));
        }

        public IEnumerable<Type> Collections()
        {
            return ExtractGeneric(_typeProvider.OurTypes, typeof(ICollectionResolvable<>));
        }

        public IEnumerable<Type> External()
        {
            return ExtractGeneric(_typeProvider.AllLoadedTypes, typeof(IExternalResolvable<>));
        }

        public IEnumerable<Type> Decorators()
        {
            return ExtractDecorators(typeof(IDecorator<>));
        }

        public IEnumerable<Type> CollectionDecorators()
        {
            return ExtractDecorators(typeof(ICollectionDecorator<>));
        }

        private IEnumerable<Type> ExtractDecorators(Type decorator)
        {
            return _typeProvider
                  .OurTypes
                  .Where(t => t.IsConcreteType()
                           && t.IsSubclassOfOpenGeneric(decorator));
        }

        private static IEnumerable<Type> ExtractGeneric(IEnumerable<Type> source, Type openGenericService)
        {
            return source
                  .Where(type => type.IsConcreteType()
                              && type.IsSubclassOfOpenGeneric(openGenericService))
                  .SelectMany(type => type.ExtractGenericArgumentsAt(openGenericService, 0))
                  .Where(type => !type.IsGenericParameter)
                  .Select(type => type.GenericTypeDefinitionOrSelf())
                  .Distinct();
        }
    }
}