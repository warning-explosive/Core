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

        private IReadOnlyCollection<ComponentOverrideInfo>? _overrides;

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
                return ApplyOverrides(Aggregate(_registrations, container => container.Resolvable())).ToList();
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
                return ApplyOverrides(Aggregate(_registrations, container => container.Collections())).ToList();
            }
        }

        public IEnumerable<DecoratorRegistrationInfo> Decorators()
        {
            _decorators ??= InitDecorators();
            return _decorators;

            IReadOnlyCollection<DecoratorRegistrationInfo> InitDecorators()
            {
                return ApplyOverrides(Aggregate(_registrations, container => container.Decorators())).ToList();
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
            Func<ComponentOverrideInfo, T> keyProducer,
            Func<T, ComponentOverrideInfo, T> overrideProducer,
            IEqualityComparer<T> comparer)
        {
            _overrides ??= InitOverrides();

            return source.FullOuterJoin(_overrides,
                left => left,
                keyProducer,
                ApplyOverride(overrideProducer),
                comparer);

            IReadOnlyCollection<ComponentOverrideInfo> InitOverrides()
            {
                return _overridesContainer
                    .Overrides
                    .GroupBy(@override => new { @override.Service, @override.Implementation })
                    .Select(grp => grp.Last())
                    .ToList();
            }
        }

        private static Func<T?, ComponentOverrideInfo?, T> ApplyOverride<T>(Func<T, ComponentOverrideInfo, T> producer)
        {
            return (registration, @override) =>
            {
                if (registration != null && @override == null)
                {
                    return registration;
                }

                if (registration != null && @override != null)
                {
                    return producer(registration, @override);
                }

                if (registration == null && @override != null)
                {
                    throw new InvalidOperationException($"Can't apply override {@override} to any registration");
                }

                throw new InvalidOperationException("Wrong override configuration");
            };
        }

        private IEnumerable<ServiceRegistrationInfo> ApplyOverrides(IEnumerable<ServiceRegistrationInfo> source)
        {
            return ApplyOverrides(
                source,
                ServiceRegistrationInfoKeyProducer,
                ServiceRegistrationInfoOverrideProducer,
                ServiceRegistrationInfo.OverrideComparer);

            static ServiceRegistrationInfo ServiceRegistrationInfoKeyProducer(ComponentOverrideInfo @override)
            {
                return new ServiceRegistrationInfo(@override.Service, @override.Implementation, @override.Lifestyle);
            }

            static ServiceRegistrationInfo ServiceRegistrationInfoOverrideProducer(
                ServiceRegistrationInfo registration,
                ComponentOverrideInfo @override)
            {
                return new ServiceRegistrationInfo(@override.Service, @override.Replacement, @override.Lifestyle);
            }
        }

        private IEnumerable<DecoratorRegistrationInfo> ApplyOverrides(IEnumerable<DecoratorRegistrationInfo> source)
        {
            return ApplyOverrides(
                source,
                DecoratorRegistrationInfoKeyProducer,
                DecoratorRegistrationInfoOverrideProducer,
                DecoratorRegistrationInfo.OverrideComparer);

            static DecoratorRegistrationInfo DecoratorRegistrationInfoKeyProducer(ComponentOverrideInfo @override)
            {
                return new DecoratorRegistrationInfo(@override.Service, @override.Implementation, @override.Lifestyle);
            }

            static DecoratorRegistrationInfo DecoratorRegistrationInfoOverrideProducer(
                DecoratorRegistrationInfo registration,
                ComponentOverrideInfo @override)
            {
                return new DecoratorRegistrationInfo(@override.Service, @override.Replacement, @override.Lifestyle);
            }
        }
    }
}