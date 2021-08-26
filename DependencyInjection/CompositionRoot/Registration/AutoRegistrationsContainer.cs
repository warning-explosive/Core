namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;
    using Extensions;

    [UnregisteredComponent]
    internal class AutoRegistrationsContainer : IRegistrationsContainer
    {
        private readonly ITypeProvider _typeProvider;
        private readonly IAutoRegistrationServicesProvider _servicesProvider;
        private readonly IConstructorResolutionBehavior _constructorResolutionBehavior;

        private IReadOnlyCollection<ServiceRegistrationInfo>? _resolvable;
        private IReadOnlyCollection<ServiceRegistrationInfo>? _collections;
        private IReadOnlyCollection<DecoratorRegistrationInfo>? _decorators;

        public AutoRegistrationsContainer(
            ITypeProvider typeProvider,
            IAutoRegistrationServicesProvider servicesProvider,
            IConstructorResolutionBehavior constructorResolutionBehavior)
        {
            _typeProvider = typeProvider;
            _servicesProvider = servicesProvider;
            _constructorResolutionBehavior = constructorResolutionBehavior;
        }

        public IEnumerable<(Type Type, object Instance)> Singletons()
        {
            return Enumerable.Empty<(Type, object)>();
        }

        public IEnumerable<ServiceRegistrationInfo> Resolvable()
        {
            _resolvable ??= InitResolvable();
            return _resolvable;
        }

        public IEnumerable<DelegateRegistrationInfo> Delegates()
        {
            return Enumerable.Empty<DelegateRegistrationInfo>();
        }

        public IEnumerable<ServiceRegistrationInfo> Collections()
        {
            _collections ??= InitCollections();
            return _collections;
        }

        public IEnumerable<DecoratorRegistrationInfo> Decorators()
        {
            _decorators ??= InitDecorators();
            return _decorators;
        }

        private IReadOnlyCollection<ServiceRegistrationInfo> InitResolvable()
        {
            var resolvable = _servicesProvider
                .Resolvable()
                .GetComponents(_typeProvider, _constructorResolutionBehavior);

            var externalResolvable = _servicesProvider
                .External()
                .GetComponents(_typeProvider, _constructorResolutionBehavior)
                .Where(info => info.Implementation.IsSubclassOfOpenGeneric(typeof(IExternalResolvable<>)));

            return resolvable
                .Concat(externalResolvable)
                .ToList();
        }

        private IReadOnlyCollection<ServiceRegistrationInfo> InitCollections()
        {
            return _servicesProvider
                .Collections()
                .GetComponents(_typeProvider, _constructorResolutionBehavior)
                .Where(info => info.Implementation.IsSubclassOfOpenGeneric(typeof(ICollectionResolvable<>)))
                .ToList();
        }

        private IReadOnlyCollection<DecoratorRegistrationInfo> InitDecorators()
        {
            var decorators = _servicesProvider
                .Decorators()
                .GetDecoratorInfo(typeof(IDecorator<>));

            var collectionDecorators = _servicesProvider
                .CollectionDecorators()
                .GetDecoratorInfo(typeof(ICollectionDecorator<>));

            return decorators
                .Concat(collectionDecorators)
                .ToList();
        }
    }
}