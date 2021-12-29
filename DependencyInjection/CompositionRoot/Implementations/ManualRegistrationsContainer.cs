namespace SpaceEngineers.Core.CompositionRoot.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Api.Abstractions;
    using Api.Abstractions.Container;
    using Api.Abstractions.Registration;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    [UnregisteredComponent]
    internal class ManualRegistrationsContainer : IRegistrationsContainer,
                                                  IAdvancedManualRegistrationsContainer
    {
        private readonly List<InstanceRegistrationInfo> _instances;
        private readonly List<ServiceRegistrationInfo> _resolvable;
        private readonly List<DelegateRegistrationInfo> _delegates;
        private readonly List<ServiceRegistrationInfo> _collections;
        private readonly List<DecoratorRegistrationInfo> _decorators;

        public ManualRegistrationsContainer(
            IDependencyContainerImplementation container,
            ITypeProvider typeProvider)
        {
            Container = container;
            Types = typeProvider;

            _instances = new List<InstanceRegistrationInfo>();
            _resolvable = new List<ServiceRegistrationInfo>();
            _delegates = new List<DelegateRegistrationInfo>();
            _collections = new List<ServiceRegistrationInfo>();
            _decorators = new List<DecoratorRegistrationInfo>();
        }

        public IDependencyContainerImplementation Container { get; }

        #region IRegistrationsContainer

        public IAdvancedManualRegistrationsContainer Advanced => this;

        public ITypeProvider Types { get; }

        public IEnumerable<InstanceRegistrationInfo> Instances()
        {
            return _instances;
        }

        public IEnumerable<ServiceRegistrationInfo> Resolvable()
        {
            return _resolvable;
        }

        public IEnumerable<DelegateRegistrationInfo> Delegates()
        {
            return _delegates;
        }

        public IEnumerable<ServiceRegistrationInfo> Collections()
        {
            return _collections;
        }

        public IEnumerable<DecoratorRegistrationInfo> Decorators()
        {
            return _decorators;
        }

        #endregion

        #region IManualRegistrationsContainer

        public IManualRegistrationsContainer Register(Type service, Type implementation, EnLifestyle lifestyle)
        {
            var info = new ServiceRegistrationInfo(service, implementation, lifestyle);
            _resolvable.Add(info);
            return this;
        }

        public IManualRegistrationsContainer Register<TService, TImplementation>(EnLifestyle lifestyle)
            where TService : class
            where TImplementation : class, TService
        {
            return Register(typeof(TService), typeof(TImplementation), lifestyle);
        }

        public IManualRegistrationsContainer RegisterInstance<TService>(TService singletonInstance)
            where TService : class
        {
            return RegisterInstance(typeof(TService), singletonInstance);
        }

        public IManualRegistrationsContainer RegisterInstance(Type service, object singletonInstance)
        {
            _instances.Add(new InstanceRegistrationInfo(service, singletonInstance));
            return this;
        }

        public IManualRegistrationsContainer RegisterDelegate<TService>(Func<TService> factory, EnLifestyle lifestyle)
            where TService : class
        {
            return RegisterDelegate(typeof(TService), factory, lifestyle);
        }

        public IManualRegistrationsContainer RegisterDelegate(Type service, Func<object> factory, EnLifestyle lifestyle)
        {
            var info = new DelegateRegistrationInfo(service, factory, lifestyle);
            _delegates.Add(info);
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
            var info = new DecoratorRegistrationInfo(service, decorator, lifestyle);

            _decorators.Add(info);

            return this;
        }

        public IManualRegistrationsContainer RegisterCollection<TService>(IEnumerable<Type> implementations, EnLifestyle lifestyle)
            where TService : class
        {
            return RegisterCollection(typeof(TService), implementations, lifestyle);
        }

        public IManualRegistrationsContainer RegisterCollection(Type service, IEnumerable<Type> implementations, EnLifestyle lifestyle)
        {
            implementations
                .Select(implementation => new ServiceRegistrationInfo(service, implementation, lifestyle))
                .Each(_collections.Add);

            return this;
        }

        #endregion
    }
}