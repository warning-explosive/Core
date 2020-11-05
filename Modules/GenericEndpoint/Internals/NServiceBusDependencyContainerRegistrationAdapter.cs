﻿namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    using System;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Enumerations;
    using NServiceBus;
    using NServiceBus.ObjectBuilder;

    /// <summary>
    /// Registration adapter for IDependencyContainer managed in external mode
    /// </summary>
    internal class NServiceBusDependencyContainerRegistrationAdapter : IConfigureComponents
    {
        private readonly IRegistrationContainer _registration;
        private readonly IBuilder _endpointBuilder;

        public NServiceBusDependencyContainerRegistrationAdapter(IRegistrationContainer registration,
                                                                 IBuilder endpointBuilder)
        {
            _registration = registration;
            _endpointBuilder = endpointBuilder;
        }

        public void ConfigureComponent(Type concreteComponent, DependencyLifecycle dependencyLifecycle)
        {
            _registration.Register(concreteComponent, concreteComponent, dependencyLifecycle.MapLifestyle());
        }

        public void ConfigureComponent<T>(DependencyLifecycle dependencyLifecycle)
        {
            _registration.Register(typeof(T), typeof(T), dependencyLifecycle.MapLifestyle());
        }

        public void ConfigureComponent<T>(Func<T> componentFactory, DependencyLifecycle dependencyLifecycle)
        {
            _registration.Register(typeof(T), () => (object)componentFactory() !, dependencyLifecycle.MapLifestyle());
        }

        public void ConfigureComponent<T>(Func<IBuilder, T> componentFactory, DependencyLifecycle dependencyLifecycle)
        {
            _registration.Register(typeof(T), () => (object)componentFactory(_endpointBuilder) !, dependencyLifecycle.MapLifestyle());
        }

        public void RegisterSingleton(Type lookupType, object instance)
        {
            _registration.Register(lookupType, () => instance, EnLifestyle.Singleton);
        }

        public void RegisterSingleton<T>(T instance)
        {
            _registration.Register(typeof(T), () => (object)instance!, EnLifestyle.Singleton);
        }

        public bool HasComponent<T>()
        {
            return _registration.HasRegistration(typeof(T));
        }

        public bool HasComponent(Type componentType)
        {
            return _registration.HasRegistration(componentType);
        }
    }
}