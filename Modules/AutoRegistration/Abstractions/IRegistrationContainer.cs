namespace SpaceEngineers.Core.AutoRegistration.Abstractions
{
    using System;
    using System.Collections.Generic;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Enumerations;

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

        /// <summary> Register implementation type </summary>
        /// <param name="implementationType">Implementation type</param>
        /// <param name="lifestyle">Instance lifestyle</param>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer RegisterImplementation(Type implementationType, EnLifestyle lifestyle);

        /// <summary> Register implementation type </summary>
        /// <param name="lifestyle">Instance lifestyle</param>
        /// <typeparam name="TImplementation">TImplementation type-argument</typeparam>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer RegisterImplementation<TImplementation>(EnLifestyle lifestyle)
            where TImplementation : class;

        /// <summary> Register collection of services </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="implementationsCollection">Collection of implementations</param>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer RegisterCollection(Type serviceType, IEnumerable<object> implementationsCollection);

        /// <summary> Register collection of services </summary>
        /// <param name="implementationsCollection">Collection of implementations</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer RegisterCollection<TService>(IEnumerable<TService> implementationsCollection)
            where TService : class;

        /// <summary>
        /// Register version
        /// Use this method only to register closed version of open-generic service
        /// </summary>
        /// <param name="lifestyle">Instance lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer RegisterVersion<TService>(EnLifestyle lifestyle)
            where TService : class;

        /// <summary>
        /// Register version
        /// Use this method only to register closed version of open-generic service
        /// </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="lifestyle">Instance lifestyle</param>
        /// <returns>IRegistrationContainer</returns>
        IRegistrationContainer RegisterVersion(Type serviceType, EnLifestyle lifestyle);

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