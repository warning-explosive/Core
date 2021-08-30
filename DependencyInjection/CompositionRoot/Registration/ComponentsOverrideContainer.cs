namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [ManuallyRegisteredComponent("Is created manually and implicitly during DependencyContainer initialization")]
    internal class ComponentsOverrideContainer : IComponentsOverrideContainer,
                                                 Abstractions.IComponentsOverrideContainer
    {
        private readonly IConstructorResolutionBehavior _constructorResolutionBehavior;

        private readonly List<ComponentOverrideInfo> _resolvableOverridesStore;
        private readonly List<ComponentOverrideInfo> _collectionResolvableOverridesStore;
        private readonly List<ComponentOverrideInfo> _decoratorOverridesStore;

        private IReadOnlyCollection<ComponentOverrideInfo>? _resolvableOverrides;
        private IReadOnlyCollection<ComponentOverrideInfo>? _collectionResolvableOverrides;
        private IReadOnlyCollection<ComponentOverrideInfo>? _decoratorOverrides;

        public ComponentsOverrideContainer(IConstructorResolutionBehavior constructorResolutionBehavior)
        {
            _constructorResolutionBehavior = constructorResolutionBehavior;

            _resolvableOverridesStore = new List<ComponentOverrideInfo>();
            _collectionResolvableOverridesStore = new List<ComponentOverrideInfo>();
            _decoratorOverridesStore = new List<ComponentOverrideInfo>();
        }

        public IEnumerable<ComponentOverrideInfo> AllOverrides => ResolvableOverrides
            .Concat(CollectionResolvableOverrides)
            .Concat(DecoratorOverrides);

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

        public IComponentsOverrideContainer Override<TService, TImplementation, TReplacement>(EnLifestyle lifestyle)
            where TImplementation : TService
            where TReplacement : TService
        {
            return Override(typeof(TService), typeof(TImplementation), typeof(TReplacement), lifestyle);
        }

        public IComponentsOverrideContainer Override(
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

        private static IReadOnlyCollection<ComponentOverrideInfo> RemoveDuplicates(IEnumerable<ComponentOverrideInfo> infos)
        {
            return infos
                .GroupBy(info => new { info.Service, info.Implementation })
                .Select(grp => grp.Last())
                .ToList();
        }
    }
}