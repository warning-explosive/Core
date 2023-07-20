namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [ManuallyRegisteredComponent("Is created manually and implicitly during DependencyContainer initialization")]
    internal class ComponentsOverrideContainer : IComponentsOverrideContainer,
                                                 IRegisterComponentsOverrideContainer,
                                                 IResolvable<IComponentsOverrideContainer>
    {
        private readonly IConstructorResolutionBehavior _constructorResolutionBehavior;

        private readonly List<IRegistrationInfo> _resolvableOverridesStore;
        private readonly Dictionary<Type, IRegistrationInfo[]> _collectionOverridesStore;
        private readonly List<DecoratorRegistrationInfo> _decoratorOverridesStore;

        private IReadOnlyCollection<IRegistrationInfo>? _resolvableOverrides;
        private IReadOnlyDictionary<Type, IRegistrationInfo[]>? _collectionOverrides;
        private IReadOnlyCollection<DecoratorRegistrationInfo>? _decoratorOverrides;

        public ComponentsOverrideContainer(IConstructorResolutionBehavior constructorResolutionBehavior)
        {
            _constructorResolutionBehavior = constructorResolutionBehavior;

            _resolvableOverridesStore = new List<IRegistrationInfo>();
            _collectionOverridesStore = new Dictionary<Type, IRegistrationInfo[]>();
            _decoratorOverridesStore = new List<DecoratorRegistrationInfo>();
        }

        public IReadOnlyCollection<IRegistrationInfo> ResolvableOverrides
        {
            get
            {
                _resolvableOverrides ??= RemoveDuplicates(
                        _resolvableOverridesStore,
                        registrationInfo => registrationInfo.Service)
                    .ToList();

                return _resolvableOverrides;
            }
        }

        public IReadOnlyDictionary<Type, IRegistrationInfo[]> CollectionOverrides
        {
            get
            {
                _collectionOverrides ??= RemoveDuplicates(
                    _collectionOverridesStore,
                    collection => collection.Key)
                    .ToDictionary(
                        collection => collection.Key,
                        collection => collection.Value);

                return _collectionOverrides;
            }
        }

        public IReadOnlyCollection<DecoratorRegistrationInfo> DecoratorOverrides
        {
            get
            {
                _decoratorOverrides ??= RemoveDuplicates(
                        _decoratorOverridesStore,
                        registrationInfo => registrationInfo.Service)
                    .ToList();

                return _decoratorOverrides;
            }
        }

        public IRegisterComponentsOverrideContainer Override<TService, TReplacement>(EnLifestyle lifestyle)
            where TReplacement : TService
        {
            return Override(typeof(TService), typeof(TReplacement), lifestyle);
        }

        public IRegisterComponentsOverrideContainer Override(
            Type service,
            Type replacement,
            EnLifestyle lifestyle)
        {
            if (replacement.IsSubclassOfOpenGeneric(typeof(IDecorator<>))
                || (_constructorResolutionBehavior.TryGetConstructor(replacement, out var cctor)
                    && cctor.IsDecorator(service)))
            {
                _decoratorOverridesStore.Add(new DecoratorRegistrationInfo(service, replacement, lifestyle));
                return this;
            }

            if (replacement.IsSubclassOfOpenGeneric(typeof(IResolvable<>)))
            {
                _resolvableOverridesStore.Add(new ServiceRegistrationInfo(service, replacement, lifestyle));
                return this;
            }

            throw new InvalidOperationException($"You can't use {replacement} as component override and register it as {service} with {lifestyle} lifestyle");
        }

        public IRegisterComponentsOverrideContainer OverrideInstance<TService>(TService replacement)
            where TService : notnull
        {
            return OverrideInstance(typeof(TService), replacement);
        }

        public IRegisterComponentsOverrideContainer OverrideInstance(Type service, object replacement)
        {
            _resolvableOverridesStore.Add(new InstanceRegistrationInfo(service, replacement));

            return this;
        }

        public IRegisterComponentsOverrideContainer OverrideDelegate<TService>(Func<TService> replacement, EnLifestyle lifestyle)
            where TService : class
        {
            return OverrideDelegate(typeof(TService), replacement, lifestyle);
        }

        public IRegisterComponentsOverrideContainer OverrideDelegate(Type service, Func<object> replacement, EnLifestyle lifestyle)
        {
            _resolvableOverridesStore.Add(new DelegateRegistrationInfo(service, replacement, lifestyle));

            return this;
        }

        public IRegisterComponentsOverrideContainer OverrideCollection<TService>(
            IEnumerable<object> instanceReplacements,
            IEnumerable<(Type implementation, EnLifestyle lifestyle)> replacements,
            IEnumerable<(Func<TService> instanceProducer, EnLifestyle lifestyle)> instanceProducerReplacements)
            where TService : class
        {
            return OverrideCollection(
                typeof(TService),
                instanceReplacements,
                replacements,
                instanceProducerReplacements.Select(replacement => ((Func<object>)replacement.instanceProducer, replacement.lifestyle)));
        }

        public IRegisterComponentsOverrideContainer OverrideCollection(
            Type service,
            IEnumerable<object> instanceReplacements,
            IEnumerable<(Type implementation, EnLifestyle lifestyle)> replacements,
            IEnumerable<(Func<object> instanceProducer, EnLifestyle lifestyle)> instanceProducerReplacements)
        {
            var collection = instanceReplacements
                .Select(replacement => new InstanceRegistrationInfo(service, replacement))
                .Cast<IRegistrationInfo>()
                .Concat(replacements
                    .Select(replacement => new ServiceRegistrationInfo(service, replacement.implementation, replacement.lifestyle)))
                .Concat(instanceProducerReplacements
                    .Select(replacement => new DelegateRegistrationInfo(service, replacement.instanceProducer, replacement.lifestyle)))
                .ToArray();

            _collectionOverridesStore.Add(service, collection);

            return this;
        }

        private static IEnumerable<T> RemoveDuplicates<T>(
            IEnumerable<T> source,
            Func<T, Type> serviceTypeAccessor)
        {
            return source
                .GroupBy(serviceTypeAccessor)
                .Select(grp => grp.Last());
        }
    }
}