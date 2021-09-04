namespace SpaceEngineers.Core.CompositionRoot.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Abstractions.Registration;
    using AutoRegistration.Api.Attributes;
    using Basics;

    [ManuallyRegisteredComponent("Is created manually and implicitly during DependencyContainer initialization")]
    internal class CompositeRegistrationsContainer : IRegistrationsContainer
    {
        private readonly IComponentsOverrideContainer _overridesContainer;
        private readonly IEnumerable<IRegistrationsContainer> _registrations;

        private IReadOnlyCollection<InstanceRegistrationInfo>? _instances;
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

        public IEnumerable<InstanceRegistrationInfo> Instances()
        {
            _instances ??= InitInstances();
            return _instances;

            IReadOnlyCollection<InstanceRegistrationInfo> InitInstances()
            {
                return ApplyOverrides(
                        Aggregate(_registrations, container => container.Instances()),
                        registrationInfo => new { registrationInfo.Service },
                        container => container.InstanceOverrides,
                        overrideInfo => new { overrideInfo.Service },
                        InstanceOverrideProducer)
                    .ToList();
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
                        registrationInfo => new { registrationInfo.Service, registrationInfo.Implementation },
                        container => container.ResolvableOverrides,
                        overrideInfo => new { overrideInfo.Service, overrideInfo.Implementation },
                        ResolvableOverrideProducer(overrideInfo => new ServiceRegistrationInfo(overrideInfo.Service, overrideInfo.Replacement, overrideInfo.Lifestyle)))
                    .ToList();
            }
        }

        public IEnumerable<DelegateRegistrationInfo> Delegates()
        {
            _delegates ??= InitDelegates();
            return _delegates;

            IReadOnlyCollection<DelegateRegistrationInfo> InitDelegates()
            {
                return ApplyOverrides(
                        Aggregate(_registrations, container => container.Delegates()),
                        registrationInfo => new { registrationInfo.Service },
                        container => container.DelegateOverrides,
                        overrideInfo => new { overrideInfo.Service },
                        DelegateOverrideProducer(overrideInfo => overrideInfo))
                    .ToList();
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
                        registrationInfo => new { registrationInfo.Service, registrationInfo.Implementation },
                        container => container.CollectionResolvableOverrides,
                        overrideInfo => new { overrideInfo.Service, overrideInfo.Implementation },
                        ResolvableOverrideProducer(overrideInfo => new ServiceRegistrationInfo(overrideInfo.Service, overrideInfo.Replacement, overrideInfo.Lifestyle)))
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
                        registrationInfo => new { registrationInfo.Service, registrationInfo.Implementation },
                        container => container.DecoratorOverrides,
                        overrideInfo => new { overrideInfo.Service, overrideInfo.Implementation },
                        DecoratorOverrideProducer(overrideInfo => new DecoratorRegistrationInfo(overrideInfo.Service, overrideInfo.Replacement, overrideInfo.Lifestyle)))
                    .ToList();
            }
        }

        private static IEnumerable<T> Aggregate<T>(
            IEnumerable<IRegistrationsContainer> registrations,
            Func<IRegistrationsContainer, IEnumerable<T>> accessor)
        {
            return registrations.SelectMany(accessor);
        }

        private IEnumerable<TSource> ApplyOverrides<TSource, TOverride, TKey>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> sourceKeySelector,
            Func<IComponentsOverrideContainer, IReadOnlyCollection<TOverride>> overrides,
            Func<TOverride, TKey> overridesKeySelector,
            Func<TSource, TOverride, TSource> overrideProducer)
        {
            return source.FullOuterJoin(
                    overrides(_overridesContainer),
                    sourceKeySelector,
                    overridesKeySelector,
                    ApplyOverride(overrideProducer));
        }

        private static Func<TSource?, TOverride?, TSource> ApplyOverride<TSource, TOverride>(Func<TSource, TOverride, TSource> producer)
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

        private static InstanceRegistrationInfo InstanceOverrideProducer(
            InstanceRegistrationInfo registrationInfo,
            InstanceRegistrationInfo overrideInfo)
        {
            return overrideInfo;
        }

        private static Func<DelegateRegistrationInfo, DelegateRegistrationInfo, DelegateRegistrationInfo> DelegateOverrideProducer(
            Func<DelegateRegistrationInfo, DelegateRegistrationInfo> producer)
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

        private static Func<ServiceRegistrationInfo, ComponentOverrideInfo, ServiceRegistrationInfo> ResolvableOverrideProducer(
            Func<ComponentOverrideInfo, ServiceRegistrationInfo> producer)
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

        private static Func<DecoratorRegistrationInfo, ComponentOverrideInfo, DecoratorRegistrationInfo> DecoratorOverrideProducer(
            Func<ComponentOverrideInfo, DecoratorRegistrationInfo> producer)
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