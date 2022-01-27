namespace SpaceEngineers.Core.CompositionRoot.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using Api.Abstractions;
    using Api.Abstractions.Registration;
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

        public IEnumerable<InstanceRegistrationInfo> Instances()
        {
            return Enumerable.Empty<InstanceRegistrationInfo>();
        }

        public IEnumerable<ServiceRegistrationInfo> Resolvable()
        {
            _resolvable ??= InitResolvable();
            return _resolvable;

            IReadOnlyCollection<ServiceRegistrationInfo> InitResolvable()
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
        }

        public IEnumerable<DelegateRegistrationInfo> Delegates()
        {
            return Enumerable.Empty<DelegateRegistrationInfo>();
        }

        public IEnumerable<IRegistrationInfo> Collections()
        {
            _collections ??= InitCollections();
            return _collections;

            IReadOnlyCollection<ServiceRegistrationInfo> InitCollections()
            {
                return _servicesProvider
                    .Collections()
                    .GetComponents(_typeProvider, _constructorResolutionBehavior)
                    .Where(info => info.Implementation.IsSubclassOfOpenGeneric(typeof(ICollectionResolvable<>)))
                    .ToList();
            }
        }

        public IEnumerable<DecoratorRegistrationInfo> Decorators()
        {
            _decorators ??= InitDecorators();
            return _decorators;

            IReadOnlyCollection<DecoratorRegistrationInfo> InitDecorators()
            {
                return _servicesProvider
                    .Decorators()
                    .GetDecoratorInfo(typeof(IDecorator<>))
                    .ToList();
            }
        }
    }
}