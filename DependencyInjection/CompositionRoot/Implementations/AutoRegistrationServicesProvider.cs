namespace SpaceEngineers.Core.CompositionRoot.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;

    [ManuallyRegisteredComponent("Is created manually and implicitly during DependencyContainer initialization")]
    internal class AutoRegistrationServicesProvider : IAutoRegistrationServicesProvider,
                                                      IResolvable<IAutoRegistrationServicesProvider>
    {
        private readonly ITypeProvider _typeProvider;

        public AutoRegistrationServicesProvider(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public IEnumerable<Type> Resolvable()
        {
            return ExtractGeneric(_typeProvider.OurTypes, typeof(IResolvable<>));
        }

        public IEnumerable<Type> Collections()
        {
            return ExtractGeneric(_typeProvider.OurTypes, typeof(ICollectionResolvable<>));
        }

        public IEnumerable<Type> Decorators()
        {
            return ExtractDecorators(typeof(IDecorator<>));
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
                  .Where(type => type.IsConcreteType())
                  .SelectMany(type => type.ExtractGenericArgumentsAt(openGenericService))
                  .Where(type => !type.IsGenericParameter)
                  .Select(type => type.GenericTypeDefinitionOrSelf())
                  .Distinct();
        }
    }
}