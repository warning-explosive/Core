namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Abstractions;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;

    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    internal class ManualRegistrationsContainer : IRegistrationsContainer, IManualRegistrationsContainer
    {
        private readonly List<(Type, object)> _singletons;
        private readonly List<ServiceRegistrationInfo> _registrations;
        private readonly List<ServiceRegistrationInfo> _collections;
        private readonly List<DecoratorRegistrationInfo> _decorators;

        internal ManualRegistrationsContainer()
        {
            _singletons = new List<(Type, object)>();
            _registrations = new List<ServiceRegistrationInfo>();
            _collections = new List<ServiceRegistrationInfo>();
            _decorators = new List<DecoratorRegistrationInfo>();
        }

        #region IRegistrationsContainer

        public IEnumerable<(Type, object)> Singletons()
        {
            return _singletons;
        }

        public IEnumerable<ServiceRegistrationInfo> Resolvable()
        {
            return _registrations;
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

        public IManualRegistrationsContainer Register(Type serviceType, Type implementationType)
        {
            var info = new ServiceRegistrationInfo(serviceType, implementationType);
            _registrations.Add(info);
            return this;
        }

        public IManualRegistrationsContainer Register<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            Register(typeof(TService), typeof(TImplementation));
            return this;
        }

        public IManualRegistrationsContainer RegisterInstance<TService>(TService singletonInstance)
            where TService : class
        {
            RegisterInstance(typeof(TService), singletonInstance);
            return this;
        }

        public IManualRegistrationsContainer RegisterInstance(Type serviceType, object singletonInstance)
        {
            var realType = singletonInstance.GetType();
            var componentAttribute = realType.GetRequiredAttribute<ComponentAttribute>();

            if (componentAttribute.Lifestyle != EnLifestyle.Singleton)
            {
                throw new InvalidOperationException($"Instance of type {realType} should be marked with {nameof(EnLifestyle.Singleton)} lifestyle");
            }

            _singletons.Add((serviceType, singletonInstance));
            return this;
        }

        public IManualRegistrationsContainer RegisterDecorator<TService, TDecorator>()
            where TService : class
            where TDecorator : class, TService
        {
            RegisterDecorator(typeof(TService), typeof(TDecorator));
            return this;
        }

        public IManualRegistrationsContainer RegisterDecorator(Type serviceType, Type decoratorType)
        {
            if (decoratorType.IsSubclassOfOpenGeneric(typeof(IConditionalDecorator<,>)))
            {
                decoratorType
                    .ExtractGenericArgumentsAt(typeof(IConditionalDecorator<,>), 1)
                    .Select(attribute => new DecoratorRegistrationInfo(serviceType, decoratorType)
                    {
                        ConditionAttribute = attribute
                    })
                    .Each(_decorators.Add);
            }
            else
            {
                var info = new DecoratorRegistrationInfo(serviceType, decoratorType);
                _decorators.Add(info);
            }

            return this;
        }

        public IManualRegistrationsContainer RegisterCollection<TService>(IEnumerable<Type> implementations)
            where TService : class
        {
            return RegisterCollection(typeof(TService), implementations);
        }

        public IManualRegistrationsContainer RegisterCollection(Type serviceType, IEnumerable<Type> implementations)
        {
            implementations
                .Select(implementation => new ServiceRegistrationInfo(serviceType, implementation))
                .Each(_collections.Add);

            return this;
        }

        #endregion
    }
}