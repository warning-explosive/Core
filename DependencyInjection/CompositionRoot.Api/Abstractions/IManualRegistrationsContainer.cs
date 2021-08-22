namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Enumerations;

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
        /// <param name="service">Service</param>
        /// <param name="implementation">Implementation</param>
        /// <param name="lifestyle">Lifestyle</param>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer Register(Type service, Type implementation, EnLifestyle lifestyle);

        /// <summary> Register implementation of service </summary>
        /// <param name="lifestyle">Lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <typeparam name="TImplementation">TImplementation type-argument</typeparam>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer Register<TService, TImplementation>(EnLifestyle lifestyle)
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
        /// <param name="lifestyle">Lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <typeparam name="TDecorator">TDecorator type-argument</typeparam>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterDecorator<TService, TDecorator>(EnLifestyle lifestyle)
            where TService : class
            where TDecorator : class, TService;

        /// <summary> Register decorator </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="decorator">Decorator type</param>
        /// <param name="lifestyle">Lifestyle</param>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterDecorator(Type serviceType, Type decorator, EnLifestyle lifestyle);

        /// <summary> Register collection of services </summary>
        /// <param name="implementations">Collection of implementation types</param>
        /// <param name="lifestyle">Lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterCollection<TService>(IEnumerable<Type> implementations, EnLifestyle lifestyle)
            where TService : class;

        /// <summary> Register collection of services </summary>
        /// <param name="service">Service type</param>
        /// <param name="implementations">Collection of implementation types</param>
        /// <param name="lifestyle">Lifestyle</param>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterCollection(Type service, IEnumerable<Type> implementations, EnLifestyle lifestyle);
    }
}