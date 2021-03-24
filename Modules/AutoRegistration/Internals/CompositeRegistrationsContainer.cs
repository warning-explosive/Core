namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Services;
    using Basics;
    using Extensions;
    using SimpleInjector;

    [SuppressMessage("Regions", "SA1124", Justification = "Readability")]
    internal class CompositeRegistrationsContainer : IRegistrationsContainer
    {
        private readonly Container _container;
        private readonly ITypeProvider _typeProvider;
        private readonly IAutoWiringServicesProvider _servicesProvider;
        private readonly IRegistrationsContainer _registrationsContainer;

        internal CompositeRegistrationsContainer(
            Container container,
            ITypeProvider typeProvider,
            IAutoWiringServicesProvider servicesProvider,
            IRegistrationsContainer registrationsContainer)
        {
            _container = container;
            _typeProvider = typeProvider;
            _servicesProvider = servicesProvider;
            _registrationsContainer = registrationsContainer;
        }

        #region IRegistrationsContainer

        public IReadOnlyCollection<(Type, object)> Singletons()
        {
            return _registrationsContainer.Singletons();
        }

        public IReadOnlyCollection<ServiceRegistrationInfo> Resolvable()
        {
            return Resolvable(_container, _typeProvider, _servicesProvider)
                .Concat(_registrationsContainer.Resolvable())
                .ToList();
        }

        public IReadOnlyCollection<ServiceRegistrationInfo> Collections()
        {
            return Collections(_container, _typeProvider, _servicesProvider)
                .Concat(_registrationsContainer.Collections())
                .ToList();
        }

        public IReadOnlyCollection<DecoratorRegistrationInfo> Decorators()
        {
            return Decorators(_servicesProvider)
                .Concat(_registrationsContainer.Decorators())
                .ToList();
        }

        #endregion

        private static IEnumerable<ServiceRegistrationInfo> Resolvable(
            Container container,
            ITypeProvider typeProvider,
            IAutoWiringServicesProvider servicesProvider)
        {
            return servicesProvider
                .Resolvable()
                .Concat(servicesProvider.External())
                .GetComponents(container, typeProvider);
        }

        private static IEnumerable<ServiceRegistrationInfo> Collections(
            Container container,
            ITypeProvider typeProvider,
            IAutoWiringServicesProvider servicesProvider)
        {
            return servicesProvider
                .Collections()
                .GetComponents(container, typeProvider);
        }

        private static IEnumerable<DecoratorRegistrationInfo> Decorators(IAutoWiringServicesProvider servicesProvider)
        {
            var decorators = servicesProvider
                .Decorators()
                .Where(decorator => !decorator.IsSubclassOfOpenGeneric(typeof(IConditionalDecorator<,>)))
                .GetDecoratorInfo(typeof(IDecorator<>));

            var conditionalDecorators = servicesProvider
                .Decorators()
                .Where(decorator => decorator.IsSubclassOfOpenGeneric(typeof(IConditionalDecorator<,>)))
                .GetConditionalDecoratorInfo(typeof(IConditionalDecorator<,>));

            var collectionDecorators = servicesProvider
                .CollectionDecorators()
                .Where(decorator => !decorator.IsSubclassOfOpenGeneric(typeof(IConditionalCollectionDecorator<,>)))
                .GetDecoratorInfo(typeof(ICollectionDecorator<>));

            var collectionConditionalDecorators = servicesProvider
                .CollectionDecorators()
                .Where(decorator => decorator.IsSubclassOfOpenGeneric(typeof(IConditionalCollectionDecorator<,>)))
                .GetConditionalDecoratorInfo(typeof(IConditionalCollectionDecorator<,>));

            var simple = decorators.Concat(collectionDecorators);
            var conditional = conditionalDecorators.Concat(collectionConditionalDecorators);
            return simple.Concat(conditional);
        }
    }
}