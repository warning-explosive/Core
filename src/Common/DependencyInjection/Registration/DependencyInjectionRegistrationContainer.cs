namespace SpaceEngineers.Core.DependencyInjection.Registration;

using System;
using System.Collections.Generic;

internal class DependencyInjectionRegistrationContainer : IDependencyInjectionRegistrationContainer
{
    private readonly List<IDependencyInjectionDependencyRegistrationInfo> _dependencies;
    private readonly List<DecoratorDependencyInjectionRegistrationInfo> _decorators;

    public DependencyInjectionRegistrationContainer(IDependencyContainer container)
    {
        DependencyContainer = container;

        _dependencies = new List<IDependencyInjectionDependencyRegistrationInfo>();
        _decorators = new List<DecoratorDependencyInjectionRegistrationInfo>();
    }

    public IDependencyContainer DependencyContainer { get; }

    #region IDependencyInjectionRegistrationContainer

    public IReadOnlyCollection<IDependencyInjectionDependencyRegistrationInfo> Dependencies()
    {
        return _dependencies;
    }

    public IReadOnlyCollection<DecoratorDependencyInjectionRegistrationInfo> Decorators()
    {
        return _decorators;
    }

    public IDependencyInjectionRegistrationContainer RegisterInstance<TService>(TService instance)
        where TService : class
    {
        return RegisterInstance(typeof(TService), instance);
    }

    public IDependencyInjectionRegistrationContainer RegisterInstance(Type service, object instance)
    {
        _dependencies.Add(new InstanceDependencyInjectionRegistrationInfo(service, instance));
        return this;
    }

    public IDependencyInjectionRegistrationContainer Register<TService, TImplementation>(EnLifestyle lifestyle)
        where TService : class
        where TImplementation : class, TService
    {
        return Register(typeof(TService), typeof(TImplementation), lifestyle);
    }

    public IDependencyInjectionRegistrationContainer Register(Type service, Type implementation, EnLifestyle lifestyle)
    {
        _dependencies.Add(new ServiceDependencyInjectionRegistrationInfo(service, implementation, lifestyle));
        return this;
    }

    public IDependencyInjectionRegistrationContainer RegisterDelegate<TService>(Func<TService> instanceProducer, EnLifestyle lifestyle)
        where TService : class
    {
        return RegisterDelegate(typeof(TService), instanceProducer, lifestyle);
    }

    public IDependencyInjectionRegistrationContainer RegisterDelegate(Type service, Func<object> instanceProducer, EnLifestyle lifestyle)
    {
        _dependencies.Add(new DelegateDependencyInjectionRegistrationInfo(service, instanceProducer, lifestyle));
        return this;
    }

    public IDependencyInjectionRegistrationContainer RegisterDecorator<TService, TDecorator>(EnLifestyle lifestyle)
        where TService : class
        where TDecorator : class, TService
    {
        return RegisterDecorator(typeof(TService), typeof(TDecorator), lifestyle);
    }

    public IDependencyInjectionRegistrationContainer RegisterDecorator(Type service, Type decorator, EnLifestyle lifestyle)
    {
        _decorators.Add(new DecoratorDependencyInjectionRegistrationInfo(service, decorator, lifestyle));
        return this;
    }

    #endregion
}