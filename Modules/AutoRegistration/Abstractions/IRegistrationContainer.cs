namespace SpaceEngineers.Core.AutoRegistration.Abstractions
{
    using System;
    using System.Collections.Generic;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Enumerations;

    /// <summary>
    /// Registration container abstraction
    /// </summary>
    public interface IRegistrationContainer : IDependencyContainer,
                                              IResolvable
    {
        /// <summary> Register implementation of service </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="implementationType">Implementation type</param>
        /// <param name="lifestyle">Instance lifestyle</param>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer Register(Type serviceType, Type implementationType, EnLifestyle lifestyle);

        /// <summary> Register implementation of service </summary>
        /// <param name="lifestyle">Instance lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <typeparam name="TImplementation">TImplementation type-argument</typeparam>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer Register<TService, TImplementation>(EnLifestyle lifestyle)
            where TService : class
            where TImplementation : class, TService;

        /// <summary> Register factory delegate </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="factory">Instance factory</param>
        /// <param name="lifestyle">Instance lifestyle</param>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer Register(Type serviceType, Func<object> factory, EnLifestyle lifestyle);

        /// <summary> Register factory delegate </summary>
        /// <param name="factory">Instance factory</param>
        /// <param name="lifestyle">Instance lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer Register<TService>(Func<TService> factory, EnLifestyle lifestyle)
            where TService : class;

        /// <summary> Register singleton instance </summary>
        /// <param name="singletonInstance">Singleton instance</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer RegisterInstance<TService>(TService singletonInstance)
            where TService : class;

        /// <summary> Register singleton instance </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="singletonInstance">Singleton instance</param>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer RegisterInstance(Type serviceType, object singletonInstance);

        /// <summary> Register decorator </summary>
        /// <param name="lifestyle">Instance lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <typeparam name="TDecorator">TDecorator type-argument</typeparam>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer RegisterDecorator<TService, TDecorator>(EnLifestyle lifestyle)
            where TService : class
            where TDecorator : class, TService;

        /// <summary> Register decorator </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="decoratorType">Decorator type</param>
        /// <param name="lifestyle">Instance lifestyle</param>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer RegisterDecorator(Type serviceType, Type decoratorType, EnLifestyle lifestyle);

        /// <summary> Register collection of services </summary>
        /// <param name="implementations">Collection of implementation types</param>
        /// <param name="lifestyle">Instance lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer RegisterCollection<TService>(IEnumerable<Type> implementations, EnLifestyle lifestyle)
            where TService : class;

        /// <summary> Register collection of services </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="implementations">Collection of implementation types</param>
        /// <param name="lifestyle">Instance lifestyle</param>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer RegisterCollection(Type serviceType, IEnumerable<Type> implementations, EnLifestyle lifestyle);

        /// <summary> Register empty collection of services </summary>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer RegisterEmptyCollection<TService>()
            where TService : class;

        /// <summary> Register empty collection of services </summary>
        /// <param name="serviceType">Service type</param>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer RegisterEmptyCollection(Type serviceType);

        /// <summary>
        /// Register versioned service
        /// Use this method only to register closed version of open-generic service
        /// </summary>
        /// <param name="lifestyle">Instance lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer RegisterVersioned<TService>(EnLifestyle lifestyle)
            where TService : class;

        /// <summary>
        /// Register versioned service
        /// Use this method only to register closed version of open-generic service
        /// </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="lifestyle">Instance lifestyle</param>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer RegisterVersioned(Type serviceType, EnLifestyle lifestyle);

        /// <summary> Does container has registration of service type </summary>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>Container has registration of service type or not</returns>
        bool HasRegistration<TService>()
            where TService : class;

        /// <summary> Does container has registration of service type </summary>
        /// <param name="serviceType">Service type</param>
        /// <returns>Container has registration of service type or not</returns>
        bool HasRegistration(Type serviceType);
    }
}