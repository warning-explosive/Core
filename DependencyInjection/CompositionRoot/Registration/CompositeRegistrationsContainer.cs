namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;

    [ManuallyRegisteredComponent("Is created manually and implicitly during DependencyContainer initialization")]
    internal class CompositeRegistrationsContainer : IRegistrationsContainer
    {
        private readonly IComponentsOverrideContainer _overridesContainer;
        private readonly IEnumerable<IRegistrationsContainer> _registrations;

        private IReadOnlyCollection<(Type, object)>? _singletons;
        private IReadOnlyCollection<ServiceRegistrationInfo>? _resolvable;
        private IReadOnlyCollection<DelegateRegistrationInfo>? _delegates;
        private IReadOnlyCollection<ServiceRegistrationInfo>? _collections;
        private IReadOnlyCollection<DecoratorRegistrationInfo>? _decorators;

        public CompositeRegistrationsContainer(
            IComponentsOverrideContainer overridesContainer,
            params IRegistrationsContainer[] registrations)
        {
            _overridesContainer = overridesContainer;
            _registrations = registrations;
        }

        public IEnumerable<(Type Type, object Instance)> Singletons()
        {
            _singletons ??= InitSingletons();
            return _singletons;

            IReadOnlyCollection<(Type, object)> InitSingletons()
            {
                return Aggregate(_registrations, container => container.Singletons()).ToList();
            }
        }

        public IEnumerable<ServiceRegistrationInfo> Resolvable()
        {
            _resolvable ??= InitResolvable();
            return _resolvable;

            IReadOnlyCollection<ServiceRegistrationInfo> InitResolvable()
            {
                return ApplyOverrides(
                        Aggregate(_registrations, container => container.Resolvable()),
                        container => container.ResolvableOverrides,
                        OverrideProducer(overrideInfo => new ServiceRegistrationInfo(overrideInfo.Service, overrideInfo.Replacement, overrideInfo.Lifestyle)))
                    .ToList();
            }
        }

        public IEnumerable<DelegateRegistrationInfo> Delegates()
        {
            _delegates ??= InitDelegates();
            return _delegates;

            IReadOnlyCollection<DelegateRegistrationInfo> InitDelegates()
            {
                return Aggregate(_registrations, container => container.Delegates()).ToList();
            }
        }

        public IEnumerable<ServiceRegistrationInfo> Collections()
        {
            _collections ??= InitCollections();
            return _collections;

            IReadOnlyCollection<ServiceRegistrationInfo> InitCollections()
            {
                return ApplyOverrides(
                        Aggregate(_registrations, container => container.Collections()),
                        container => container.CollectionResolvableOverrides,
                        OverrideProducer(overrideInfo => new ServiceRegistrationInfo(overrideInfo.Service, overrideInfo.Replacement, overrideInfo.Lifestyle)))
                    .ToList();
            }
        }

        public IEnumerable<DecoratorRegistrationInfo> Decorators()
        {
            _decorators ??= InitDecorators();
            return _decorators;

            IReadOnlyCollection<DecoratorRegistrationInfo> InitDecorators()
            {
                return ApplyOverrides(
                        Aggregate(_registrations, container => container.Decorators()),
                        container => container.DecoratorOverrides,
                        OverrideDecoratorProducer(overrideInfo => new DecoratorRegistrationInfo(overrideInfo.Service, overrideInfo.Replacement, overrideInfo.Lifestyle)))
                    .ToList();
            }
        }

        private static IEnumerable<T> Aggregate<T>(
            IEnumerable<IRegistrationsContainer> registrations,
            Func<IRegistrationsContainer, IEnumerable<T>> accessor)
        {
            return registrations.SelectMany(accessor);
        }

        private IEnumerable<T> ApplyOverrides<T>(
            IEnumerable<T> source,
            Func<IComponentsOverrideContainer, IReadOnlyCollection<ComponentOverrideInfo>> overrides,
            Func<T, ComponentOverrideInfo, T> overrideProducer)
            where T : IComponentRegistrationInfo
        {
            return source.FullOuterJoin(
                    overrides(_overridesContainer),
                    left => new { left.Service, left.Implementation },
                    right => new { right.Service, right.Implementation },
                    ApplyOverride(overrideProducer));
        }

        private static Func<T?, ComponentOverrideInfo?, T> ApplyOverride<T>(Func<T, ComponentOverrideInfo, T> producer)
        {
            return (registrationInfo, overrideInfo) =>
            {
                if (registrationInfo != null && overrideInfo == null)
                {
                    return registrationInfo;
                }

                if (registrationInfo != null && overrideInfo != null)
                {
                    return producer(registrationInfo, overrideInfo);
                }

                if (registrationInfo == null && overrideInfo != null)
                {
                    throw new InvalidOperationException($"Can't apply override {overrideInfo} to any registration");
                }

                throw new InvalidOperationException("Wrong override configuration");
            };
        }

        private static Func<T, ComponentOverrideInfo, T> OverrideProducer<T>(
            Func<ComponentOverrideInfo, T> producer)
            where T : IComponentRegistrationInfo
        {
            return (registrationInfo, overrideInfo) =>
            {
                if (registrationInfo.Lifestyle <= overrideInfo.Lifestyle)
                {
                    return producer(overrideInfo);
                }

                throw new InvalidOperationException($"Lifestyle mismatch - applying {overrideInfo} to {registrationInfo} leads to captive dependency");
            };
        }

        private static Func<T, ComponentOverrideInfo, T> OverrideDecoratorProducer<T>(
            Func<ComponentOverrideInfo, T> producer)
            where T : IComponentRegistrationInfo
        {
            return (registrationInfo, overrideInfo) =>
            {
                if (registrationInfo.Lifestyle >= overrideInfo.Lifestyle)
                {
                    return producer(overrideInfo);
                }

                throw new InvalidOperationException($"Lifestyle mismatch - applying {overrideInfo} to {registrationInfo} leads to captive dependency");
            };
        }
    }
}