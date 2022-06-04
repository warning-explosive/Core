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

        private IReadOnlyCollection<Type>? _resolvable;
        private IReadOnlyCollection<Type>? _collections;
        private IReadOnlyCollection<Type>? _decorators;

        public AutoRegistrationServicesProvider(ITypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
        }

        public IReadOnlyCollection<Type> Resolvable()
        {
            _resolvable ??= InitResolvable();
            return _resolvable;

            IReadOnlyCollection<Type> InitResolvable()
            {
                return ExtractGeneric(_typeProvider.OurTypes, typeof(IResolvable<>)).ToList();
            }
        }

        public IReadOnlyCollection<Type> Collections()
        {
            _collections ??= InitCollections();
            return _collections;

            IReadOnlyCollection<Type> InitCollections()
            {
                return ExtractGeneric(_typeProvider.OurTypes, typeof(ICollectionResolvable<>)).ToList();
            }
        }

        public IReadOnlyCollection<Type> Decorators()
        {
            _decorators ??= InitDecorators();
            return _decorators;

            IReadOnlyCollection<Type> InitDecorators()
            {
                return ExtractDecorators(typeof(IDecorator<>)).ToList();
            }
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