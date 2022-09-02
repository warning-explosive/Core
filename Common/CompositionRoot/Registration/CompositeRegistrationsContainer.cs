namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;

    [ManuallyRegisteredComponent("Is created manually and implicitly during DependencyContainer initialization")]
    internal class CompositeRegistrationsContainer : IRegistrationsContainer,
                                                     IResolvable<IRegistrationsContainer>
    {
        private readonly IComponentsOverrideContainer _overridesContainer;
        private readonly IEnumerable<IRegistrationsContainer> _registrations;

        private IReadOnlyCollection<InstanceRegistrationInfo>? _instances;
        private IReadOnlyCollection<ServiceRegistrationInfo>? _resolvable;
        private IReadOnlyCollection<DelegateRegistrationInfo>? _delegates;
        private IReadOnlyCollection<IRegistrationInfo>? _collections;
        private IReadOnlyCollection<DecoratorRegistrationInfo>? _decorators;

        public CompositeRegistrationsContainer(
            IComponentsOverrideContainer overridesContainer,
            params IRegistrationsContainer[] registrations)
        {
            _overridesContainer = overridesContainer;
            _registrations = registrations;
        }

        public IReadOnlyCollection<InstanceRegistrationInfo> Instances()
        {
            _instances ??= Init().Instances;
            return _instances;
        }

        public IReadOnlyCollection<ServiceRegistrationInfo> Resolvable()
        {
            _resolvable ??= Init().Resolvable;
            return _resolvable;
        }

        public IReadOnlyCollection<DelegateRegistrationInfo> Delegates()
        {
            _delegates ??= Init().Delegates;
            return _delegates;
        }

        public IReadOnlyCollection<IRegistrationInfo> Collections()
        {
            _collections ??= Init().Collections;
            return _collections;
        }

        public IReadOnlyCollection<DecoratorRegistrationInfo> Decorators()
        {
            _decorators ??= Init().Decorators;
            return _decorators;
        }

        private (IReadOnlyCollection<InstanceRegistrationInfo> Instances,
            IReadOnlyCollection<ServiceRegistrationInfo> Resolvable,
            IReadOnlyCollection<DelegateRegistrationInfo> Delegates,
            IReadOnlyCollection<IRegistrationInfo> Collections,
            IReadOnlyCollection<DecoratorRegistrationInfo> Decorators)
            Init()
        {
            (_instances, _resolvable, _delegates) = OverrideResolvable();
            _collections = OverrideCollections();
            _decorators = OverrideDecorators();

            return (_instances, _resolvable, _delegates, _collections, _decorators);
        }

        private (IReadOnlyCollection<InstanceRegistrationInfo>,
            IReadOnlyCollection<ServiceRegistrationInfo>,
            IReadOnlyCollection<DelegateRegistrationInfo>)
            OverrideResolvable()
        {
            var instances = _registrations
                .SelectMany(container => container.Instances())
                .ToList();

            var resolvable = _registrations
                .SelectMany(container => container.Resolvable())
                .ToList();

            var delegates = _registrations
                .SelectMany(container => container.Delegates())
                .ToList();

            _ = instances
                .Cast<IRegistrationInfo>()
                .Concat(resolvable)
                .Concat(delegates)
                .ToList()
                .FullOuterJoin(
                    _overridesContainer.ResolvableOverrides,
                    registrationInfo => registrationInfo.Service,
                    overrideInfo => overrideInfo.Service,
                    ApplyOverride<IRegistrationInfo>(ReplaceRegistration, ServiceLifetimeComparer))
                .ToList();

            return (instances, resolvable, delegates);

            IRegistrationInfo ReplaceRegistration(
                IRegistrationInfo registrationInfo,
                IRegistrationInfo overrideInfo)
            {
                RemoveRegistration(registrationInfo);
                AddRegistration(overrideInfo);

                return overrideInfo;
            }

            void RemoveRegistration(
                IRegistrationInfo info)
            {
                switch (info)
                {
                    case InstanceRegistrationInfo instanceRegistrationInfo:
                        instances.Remove(instanceRegistrationInfo);
                        break;
                    case ServiceRegistrationInfo serviceRegistrationInfo:
                        resolvable.Remove(serviceRegistrationInfo);
                        break;
                    case DelegateRegistrationInfo delegateRegistrationInfo:
                        delegates.Remove(delegateRegistrationInfo);
                        break;
                    default:
                        throw new NotSupportedException(info.GetType().Name);
                }
            }

            void AddRegistration(IRegistrationInfo info)
            {
                switch (info)
                {
                    case InstanceRegistrationInfo instanceRegistrationInfo:
                        instances.Add(instanceRegistrationInfo);
                        break;
                    case ServiceRegistrationInfo serviceRegistrationInfo:
                        resolvable.Add(serviceRegistrationInfo);
                        break;
                    case DelegateRegistrationInfo delegateRegistrationInfo:
                        delegates.Add(delegateRegistrationInfo);
                        break;
                    default:
                        throw new NotSupportedException(info.GetType().Name);
                }
            }
        }

        private IReadOnlyCollection<IRegistrationInfo> OverrideCollections()
        {
            return _registrations
                .SelectMany(container => container.Collections())
                .GroupBy(registrationInfo => registrationInfo.Service)
                .FullOuterJoin(
                    _overridesContainer.CollectionOverrides,
                    registrationInfo => registrationInfo.Key,
                    overrideInfo => overrideInfo.Key,
                    (registrationInfo, overrideInfo) => ApplyCollectionOverride(registrationInfo, overrideInfo.Value))
                .SelectMany(registrationInfo => registrationInfo)
                .ToList();

            static IEnumerable<IRegistrationInfo> ApplyCollectionOverride(
                IEnumerable<IRegistrationInfo>? registeredCollection,
                IEnumerable<IRegistrationInfo>? overrideCollection)
            {
                if (registeredCollection != null && overrideCollection != null)
                {
                    return overrideCollection;
                }

                if (registeredCollection != null && overrideCollection == null)
                {
                    return registeredCollection;
                }

                if (registeredCollection == null && overrideCollection != null)
                {
                    throw new InvalidOperationException($"Can't apply override {overrideCollection} to any registration");
                }

                throw new InvalidOperationException("Wrong override configuration");
            }
        }

        private IReadOnlyCollection<DecoratorRegistrationInfo> OverrideDecorators()
        {
            return _registrations
                .SelectMany(container => container.Decorators())
                .FullOuterJoin(
                    _overridesContainer.DecoratorOverrides,
                    registrationInfo => registrationInfo.Service,
                    overrideInfo => overrideInfo.Service,
                    ApplyOverride<DecoratorRegistrationInfo>(ReplaceRegistration, DecoratorLifetimeComparer))
                .ToList();

            static DecoratorRegistrationInfo ReplaceRegistration(
                DecoratorRegistrationInfo registrationInfo,
                DecoratorRegistrationInfo overrideInfo)
            {
                return overrideInfo;
            }
        }

        private static Func<TInfo?, TInfo?, TInfo> ApplyOverride<TInfo>(
            Func<TInfo, TInfo, TInfo> producer,
            Func<TInfo, TInfo, bool> lifestylesComparer)
            where TInfo : IRegistrationInfo
        {
            return (registrationInfo, overrideInfo) =>
            {
                if (registrationInfo != null && overrideInfo != null)
                {
                    if (lifestylesComparer(registrationInfo, overrideInfo))
                    {
                        return producer(registrationInfo, overrideInfo);
                    }

                    throw new InvalidOperationException($"Lifestyle mismatch - applying {overrideInfo} to {registrationInfo} leads to captive dependency");
                }

                if (registrationInfo != null && overrideInfo == null)
                {
                    return registrationInfo;
                }

                if (registrationInfo == null && overrideInfo != null)
                {
                    throw new InvalidOperationException($"Can't apply override {overrideInfo} to any registration");
                }

                throw new InvalidOperationException("Wrong override configuration");
            };
        }

        private static bool ServiceLifetimeComparer(
            IRegistrationInfo registrationInfo,
            IRegistrationInfo overrideInfo)
        {
            return registrationInfo.Lifestyle <= overrideInfo.Lifestyle;
        }

        private static bool DecoratorLifetimeComparer(
            DecoratorRegistrationInfo registrationInfo,
            DecoratorRegistrationInfo overrideInfo)
        {
            return registrationInfo.Lifestyle >= overrideInfo.Lifestyle;
        }
    }
}