namespace SpaceEngineers.Core.CompositionRoot.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Abstractions;
    using Api.Abstractions.Registration;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [ManuallyRegisteredComponent("Is created manually and implicitly during DependencyContainer initialization")]
    internal class ComponentsOverrideContainer : IComponentsOverrideContainer,
                                                 IRegisterComponentsOverrideContainer
    {
        private readonly IConstructorResolutionBehavior _constructorResolutionBehavior;

        private readonly List<InstanceRegistrationInfo> _instanceOverridesStore;
        private readonly List<DelegateRegistrationInfo> _delegateOverridesStore;
        private readonly List<ComponentOverrideInfo> _resolvableOverridesStore;
        private readonly List<ComponentOverrideInfo> _collectionResolvableOverridesStore;
        private readonly List<ComponentOverrideInfo> _decoratorOverridesStore;

        private IReadOnlyCollection<InstanceRegistrationInfo>? _instanceOverrides;
        private IReadOnlyCollection<DelegateRegistrationInfo>? _delegateOverrides;
        private IReadOnlyCollection<ComponentOverrideInfo>? _resolvableOverrides;
        private IReadOnlyCollection<ComponentOverrideInfo>? _collectionResolvableOverrides;
        private IReadOnlyCollection<ComponentOverrideInfo>? _decoratorOverrides;

        public ComponentsOverrideContainer(IConstructorResolutionBehavior constructorResolutionBehavior)
        {
            _constructorResolutionBehavior = constructorResolutionBehavior;

            _instanceOverridesStore = new List<InstanceRegistrationInfo>();
            _delegateOverridesStore = new List<DelegateRegistrationInfo>();
            _resolvableOverridesStore = new List<ComponentOverrideInfo>();
            _collectionResolvableOverridesStore = new List<ComponentOverrideInfo>();
            _decoratorOverridesStore = new List<ComponentOverrideInfo>();
        }

        public IEnumerable<ComponentOverrideInfo> AllOverrides => ResolvableOverrides
            .Concat(CollectionResolvableOverrides)
            .Concat(DecoratorOverrides);

        public IReadOnlyCollection<InstanceRegistrationInfo> InstanceOverrides
        {
            get
            {
                _instanceOverrides ??= RemoveDuplicates(_instanceOverridesStore);
                return _instanceOverrides;
            }
        }

        public IReadOnlyCollection<DelegateRegistrationInfo> DelegateOverrides
        {
            get
            {
                _delegateOverrides ??= RemoveDuplicates(_delegateOverridesStore);
                return _delegateOverrides;
            }
        }

        public IReadOnlyCollection<ComponentOverrideInfo> ResolvableOverrides
        {
            get
            {
                _resolvableOverrides ??= RemoveDuplicates(_resolvableOverridesStore);
                return _resolvableOverrides;
            }
        }

        public IReadOnlyCollection<ComponentOverrideInfo> CollectionResolvableOverrides
        {
            get
            {
                _collectionResolvableOverrides ??= RemoveDuplicates(_collectionResolvableOverridesStore);
                return _collectionResolvableOverrides;
            }
        }

        public IReadOnlyCollection<ComponentOverrideInfo> DecoratorOverrides
        {
            get
            {
                _decoratorOverrides ??= RemoveDuplicates(_decoratorOverridesStore);
                return _decoratorOverrides;
            }
        }

        public IRegisterComponentsOverrideContainer Override<TService, TImplementation, TReplacement>(EnLifestyle lifestyle)
            where TImplementation : TService
            where TReplacement : TService
        {
            return Override(typeof(TService), typeof(TImplementation), typeof(TReplacement), lifestyle);
        }

        public IRegisterComponentsOverrideContainer Override(
            Type service,
            Type implementation,
            Type replacement,
            EnLifestyle lifestyle)
        {
            var info = new ComponentOverrideInfo(service, implementation, replacement, lifestyle);

            if (typeof(IResolvable).IsAssignableFrom(replacement))
            {
                _resolvableOverridesStore.Add(info);
                return this;
            }

            if (replacement.IsSubclassOfOpenGeneric(typeof(ICollectionResolvable<>)))
            {
                _collectionResolvableOverridesStore.Add(info);
                return this;
            }

            if (replacement.IsSubclassOfOpenGeneric(typeof(IDecorator<>))
                || (_constructorResolutionBehavior.TryGetConstructor(replacement, out var cctor)
                    && cctor.IsDecorator(service)))
            {
                _decoratorOverridesStore.Add(info);
                return this;
            }

            throw new InvalidOperationException($"You can't use {replacement} as component override for {implementation} that have been registered as {service} with {lifestyle} lifestyle");
        }

        public IRegisterComponentsOverrideContainer OverrideInstance<TService>(TService replacement)
            where TService : notnull
        {
            return OverrideInstance(typeof(TService), replacement);
        }

        public IRegisterComponentsOverrideContainer OverrideInstance(Type service, object replacement)
        {
            _instanceOverridesStore.Add(new InstanceRegistrationInfo(service, replacement));
            return this;
        }

        public IRegisterComponentsOverrideContainer OverrideDelegate<TService>(Func<TService> replacement, EnLifestyle lifestyle)
            where TService : class
        {
            return OverrideDelegate(typeof(TService), replacement, lifestyle);
        }

        public IRegisterComponentsOverrideContainer OverrideDelegate(Type service, Func<object> replacement, EnLifestyle lifestyle)
        {
            _delegateOverridesStore.Add(new DelegateRegistrationInfo(service, replacement, lifestyle));
            return this;
        }

        private static IReadOnlyCollection<InstanceRegistrationInfo> RemoveDuplicates(IEnumerable<InstanceRegistrationInfo> infos)
        {
            return infos
                .GroupBy(info => new { info.Service })
                .Select(grp => grp.Last())
                .ToList();
        }

        private static IReadOnlyCollection<DelegateRegistrationInfo> RemoveDuplicates(IEnumerable<DelegateRegistrationInfo> infos)
        {
            return infos
                .GroupBy(info => new { info.Service })
                .Select(grp => grp.Last())
                .ToList();
        }

        private static IReadOnlyCollection<ComponentOverrideInfo> RemoveDuplicates(IEnumerable<ComponentOverrideInfo> infos)
        {
            return infos
                .GroupBy(info => new { info.Service, info.Implementation })
                .Select(grp => grp.Last())
                .ToList();
        }
    }
}