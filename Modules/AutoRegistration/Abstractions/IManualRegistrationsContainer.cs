namespace SpaceEngineers.Core.AutoRegistration.Abstractions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Manual registrations container abstraction
    /// </summary>
    public interface IManualRegistrationsContainer
    {
        /// <summary>
        /// IAdvancedManualRegistrationsContainer
        /// </summary>
        IAdvancedManualRegistrationsContainer Advanced { get; }

        /// <summary> Register implementation of service </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="implementationType">Implementation type</param>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer Register(Type serviceType, Type implementationType);

        /// <summary> Register implementation of service </summary>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <typeparam name="TImplementation">TImplementation type-argument</typeparam>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer Register<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;

        /// <summary> Register singleton instance </summary>
        /// <param name="singletonInstance">Singleton instance</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterInstance<TService>(TService singletonInstance)
            where TService : class;

        /// <summary> Register singleton instance </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="singletonInstance">Singleton instance</param>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterInstance(Type serviceType, object singletonInstance);

        /// <summary> Register decorator </summary>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <typeparam name="TDecorator">TDecorator type-argument</typeparam>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterDecorator<TService, TDecorator>()
            where TService : class
            where TDecorator : class, TService;

        /// <summary> Register decorator </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="decoratorType">Decorator type</param>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterDecorator(Type serviceType, Type decoratorType);

        /// <summary> Register collection of services </summary>
        /// <param name="implementations">Collection of implementation types</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterCollection<TService>(IEnumerable<Type> implementations)
            where TService : class;

        /// <summary> Register collection of services </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="implementations">Collection of implementation types</param>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterCollection(Type serviceType, IEnumerable<Type> implementations);
    }
}