namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    [UnregisteredComponent]
    internal class ManualRegistrationsContainer : IRegistrationsContainer,
                                                  IAdvancedManualRegistrationsContainer,
                                                  IResolvable<IRegistrationsContainer>
    {
        private readonly List<InstanceRegistrationInfo> _instances;
        private readonly List<ServiceRegistrationInfo> _resolvable;
        private readonly List<DelegateRegistrationInfo> _delegates;
        private readonly List<IRegistrationInfo> _collections;
        private readonly List<DecoratorRegistrationInfo> _decorators;

        public ManualRegistrationsContainer(
            DependencyContainer container,
            ITypeProvider typeProvider)
        {
            Container = container;
            Types = typeProvider;

            _instances = new List<InstanceRegistrationInfo>();
            _resolvable = new List<ServiceRegistrationInfo>();
            _delegates = new List<DelegateRegistrationInfo>();
            _collections = new List<IRegistrationInfo>();
            _decorators = new List<DecoratorRegistrationInfo>();
        }

        public IDependencyContainer Container { get; }

        #region IRegistrationsContainer

        public IAdvancedManualRegistrationsContainer Advanced => this;

        public ITypeProvider Types { get; }

        public IReadOnlyCollection<InstanceRegistrationInfo> Instances()
        {
            return _instances;
        }

        public IReadOnlyCollection<ServiceRegistrationInfo> Resolvable()
        {
            return _resolvable;
        }

        public IReadOnlyCollection<DelegateRegistrationInfo> Delegates()
        {
            return _delegates;
        }

        public IReadOnlyCollection<IRegistrationInfo> Collections()
        {
            return _collections;
        }

        public IReadOnlyCollection<DecoratorRegistrationInfo> Decorators()
        {
            return _decorators;
        }

        #endregion

        #region IManualRegistrationsContainer

        public IManualRegistrationsContainer Register(Type service, Type implementation, EnLifestyle lifestyle)
        {
            _resolvable.Add(new ServiceRegistrationInfo(service, implementation, lifestyle));
            return this;
        }

        public IManualRegistrationsContainer Register<TService, TImplementation>(EnLifestyle lifestyle)
            where TService : class
            where TImplementation : class, TService
        {
            return Register(typeof(TService), typeof(TImplementation), lifestyle);
        }

        public IManualRegistrationsContainer RegisterInstance<TService>(TService instance)
            where TService : class
        {
            return RegisterInstance(typeof(TService), instance);
        }

        public IManualRegistrationsContainer RegisterInstance(Type service, object instance)
        {
            _instances.Add(new InstanceRegistrationInfo(service, instance));
            return this;
        }

        public IManualRegistrationsContainer RegisterDelegate<TService>(Func<TService> instanceProducer, EnLifestyle lifestyle)
            where TService : class
        {
            return RegisterDelegate(typeof(TService), instanceProducer, lifestyle);
        }

        public IManualRegistrationsContainer RegisterDelegate(Type service, Func<object> instanceProducer, EnLifestyle lifestyle)
        {
            _delegates.Add(new DelegateRegistrationInfo(service, instanceProducer, lifestyle));
            return this;
        }

        public IManualRegistrationsContainer RegisterDecorator<TService, TDecorator>(EnLifestyle lifestyle)
            where TService : class
            where TDecorator : class, TService
        {
            return RegisterDecorator(typeof(TService), typeof(TDecorator), lifestyle);
        }

        public IManualRegistrationsContainer RegisterDecorator(Type service, Type decorator, EnLifestyle lifestyle)
        {
            _decorators.Add(new DecoratorRegistrationInfo(service, decorator, lifestyle));
            return this;
        }

        public IManualRegistrationsContainer RegisterCollectionEntry<TService, TImplementation>(EnLifestyle lifestyle)
            where TService : class
            where TImplementation : class, TService
        {
            return RegisterCollectionEntry(typeof(TService), typeof(TImplementation), lifestyle);
        }

        public IManualRegistrationsContainer RegisterCollectionEntry<TService>(Type implementation, EnLifestyle lifestyle)
            where TService : class
        {
            return RegisterCollectionEntry(typeof(TService), implementation, lifestyle);
        }

        public IManualRegistrationsContainer RegisterCollectionEntry(Type service, Type implementation, EnLifestyle lifestyle)
        {
            _collections.Add(new ServiceRegistrationInfo(service, implementation, lifestyle));
            return this;
        }

        public IManualRegistrationsContainer RegisterCollectionEntryInstance<TService>(TService instance)
            where TService : class
        {
            return RegisterCollectionEntryInstance(typeof(TService), instance);
        }

        public IManualRegistrationsContainer RegisterCollectionEntryInstance(Type service, object implementation)
        {
            _collections.Add(new InstanceRegistrationInfo(service, implementation));
            return this;
        }

        public IManualRegistrationsContainer RegisterCollectionEntryDelegate<TService>(Func<TService> instanceProducer, EnLifestyle lifestyle)
            where TService : class
        {
            return RegisterCollectionEntryDelegate(typeof(TService), instanceProducer, lifestyle);
        }

        public IManualRegistrationsContainer RegisterCollectionEntryDelegate(Type service, Func<object> instanceProducer, EnLifestyle lifestyle)
        {
            _collections.Add(new DelegateRegistrationInfo(service, instanceProducer, lifestyle));
            return this;
        }

        #endregion
    }
}