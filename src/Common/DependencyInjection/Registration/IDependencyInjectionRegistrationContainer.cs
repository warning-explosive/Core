namespace SpaceEngineers.Core.DependencyInjection.Registration;

using System;
using System.Collections.Generic;

public interface IDependencyInjectionRegistrationContainer
{
    IDependencyContainer DependencyContainer { get; }

    IReadOnlyCollection<IDependencyInjectionDependencyRegistrationInfo> Dependencies();

    IReadOnlyCollection<DecoratorDependencyInjectionRegistrationInfo> Decorators();

    IDependencyInjectionRegistrationContainer RegisterInstance<TService>(TService instance)
        where TService : class;

    IDependencyInjectionRegistrationContainer RegisterInstance(Type serviceType, object instance);

    IDependencyInjectionRegistrationContainer Register<TService, TImplementation>(EnLifestyle lifestyle)
        where TService : class
        where TImplementation : class, TService;

    IDependencyInjectionRegistrationContainer Register(Type service, Type implementation, EnLifestyle lifestyle);

    IDependencyInjectionRegistrationContainer RegisterDelegate<TService>(Func<TService> instanceProducer, EnLifestyle lifestyle)
        where TService : class;

    IDependencyInjectionRegistrationContainer RegisterDelegate(Type serviceType, Func<object> instanceProducer, EnLifestyle lifestyle);

    IDependencyInjectionRegistrationContainer RegisterDecorator<TService, TDecorator>(EnLifestyle lifestyle)
        where TService : class
        where TDecorator : class, TService;

    IDependencyInjectionRegistrationContainer RegisterDecorator(Type serviceType, Type decorator, EnLifestyle lifestyle);
}