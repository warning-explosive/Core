namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Services;
    using Basics;
    using Extensions;
    using SimpleInjector;

    internal class AutoRegistrationsContainer : IRegistrationsContainer
    {
        private readonly Container _container;
        private readonly ITypeProvider _typeProvider;
        private readonly IAutoWiringServicesProvider _servicesProvider;

        public AutoRegistrationsContainer(
            Container container,
            ITypeProvider typeProvider,
            IAutoWiringServicesProvider servicesProvider)
        {
            _container = container;
            _typeProvider = typeProvider;
            _servicesProvider = servicesProvider;
        }

        public IEnumerable<(Type, object)> Singletons()
        {
            return Enumerable.Empty<(Type, object)>();
        }

        public IEnumerable<ServiceRegistrationInfo> Resolvable()
        {
            return _servicesProvider
                .Resolvable()
                .GetComponents(_container, _typeProvider)
                .Concat(_servicesProvider
                    .External()
                    .GetComponents(_container, _typeProvider)
                    .Where(info => info.ImplementationType.IsSubclassOfOpenGeneric(typeof(IExternalResolvable<>))));
        }

        public IEnumerable<DelegateRegistrationInfo> Delegates()
        {
            return Enumerable.Empty<DelegateRegistrationInfo>();
        }

        public IEnumerable<ServiceRegistrationInfo> Collections()
        {
            return _servicesProvider
                .Collections()
                .GetComponents(_container, _typeProvider)
                .Where(info => info.ImplementationType.IsSubclassOfOpenGeneric(typeof(ICollectionResolvable<>)));
        }

        public IEnumerable<DecoratorRegistrationInfo> Decorators()
        {
            var decorators = _servicesProvider
                .Decorators()
                .Where(decorator => !decorator.IsSubclassOfOpenGeneric(typeof(IConditionalDecorator<,>)))
                .GetDecoratorInfo(typeof(IDecorator<>));

            var conditionalDecorators = _servicesProvider
                .Decorators()
                .Where(decorator => decorator.IsSubclassOfOpenGeneric(typeof(IConditionalDecorator<,>)))
                .GetConditionalDecoratorInfo(typeof(IConditionalDecorator<,>));

            var collectionDecorators = _servicesProvider
                .CollectionDecorators()
                .Where(decorator => !decorator.IsSubclassOfOpenGeneric(typeof(IConditionalCollectionDecorator<,>)))
                .GetDecoratorInfo(typeof(ICollectionDecorator<>));

            var collectionConditionalDecorators = _servicesProvider
                .CollectionDecorators()
                .Where(decorator => decorator.IsSubclassOfOpenGeneric(typeof(IConditionalCollectionDecorator<,>)))
                .GetConditionalDecoratorInfo(typeof(IConditionalCollectionDecorator<,>));

            var simple = decorators.Concat(collectionDecorators);
            var conditional = conditionalDecorators.Concat(collectionConditionalDecorators);
            return simple.Concat(conditional);
        }
    }
}