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
            return _servicesProvider
                .Resolvable()
                .GetComponents(_typeProvider, _constructorResolutionBehavior)
                .Concat(_servicesProvider
                    .External()
                    .GetComponents(_typeProvider, _constructorResolutionBehavior)
                    .Where(info => info.Implementation.IsSubclassOfOpenGeneric(typeof(IExternalResolvable<>))));
        }

        public IEnumerable<DelegateRegistrationInfo> Delegates()
        {
            return Enumerable.Empty<DelegateRegistrationInfo>();
        }

        public IEnumerable<ServiceRegistrationInfo> Collections()
        {
            return _servicesProvider
                .Collections()
                .GetComponents(_typeProvider, _constructorResolutionBehavior)
                .Where(info => info.Implementation.IsSubclassOfOpenGeneric(typeof(ICollectionResolvable<>)));
        }

        public IEnumerable<DecoratorRegistrationInfo> Decorators()
        {
            var decorators = _servicesProvider
                .Decorators()
                .GetDecoratorInfo(typeof(IDecorator<>));

            var collectionDecorators = _servicesProvider
                .CollectionDecorators()
                .GetDecoratorInfo(typeof(ICollectionDecorator<>));

            return decorators.Concat(collectionDecorators);
        }
    }
}