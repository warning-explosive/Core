namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration
{
    using System;
    using AutoRegistration.Api.Enumerations;

    /// <summary>
    /// IManualRegistrationsContainer
    /// </summary>
    public interface IManualRegistrationsContainer
    {
        /// <summary>
        /// IAdvancedManualRegistrationsContainer
        /// </summary>
        IAdvancedManualRegistrationsContainer Advanced { get; }

        /// <summary>
        /// ITypeProvider
        /// </summary>
        ITypeProvider Types { get; }

        /// <summary> Registers implementation of service </summary>
        /// <param name="lifestyle">Lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <typeparam name="TImplementation">TImplementation type-argument</typeparam>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer Register<TService, TImplementation>(EnLifestyle lifestyle)
            where TService : class
            where TImplementation : class, TService;

        /// <summary> Registers implementation of service </summary>
        /// <param name="service">Service</param>
        /// <param name="implementation">Implementation</param>
        /// <param name="lifestyle">Lifestyle</param>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer Register(Type service, Type implementation, EnLifestyle lifestyle);

        /// <summary> Registers instance </summary>
        /// <param name="instance">Instance</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterInstance<TService>(TService instance)
            where TService : class;

        /// <summary> Registers instance </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="instance">Instance</param>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterInstance(Type serviceType, object instance);

        /// <summary> Registers decorator </summary>
        /// <param name="lifestyle">Lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <typeparam name="TDecorator">TDecorator type-argument</typeparam>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterDecorator<TService, TDecorator>(EnLifestyle lifestyle)
            where TService : class
            where TDecorator : class, TService;

        /// <summary> Registers decorator </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="decorator">Decorator type</param>
        /// <param name="lifestyle">Lifestyle</param>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterDecorator(Type serviceType, Type decorator, EnLifestyle lifestyle);

        /// <summary> Register service collection entry </summary>
        /// <param name="lifestyle">Lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <typeparam name="TImplementation">TImplementation type-argument</typeparam>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterCollectionEntry<TService, TImplementation>(EnLifestyle lifestyle)
            where TService : class
            where TImplementation : class, TService;

        /// <summary> Register service collection entry </summary>
        /// <param name="implementation">Entry implementation type</param>
        /// <param name="lifestyle">Lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterCollectionEntry<TService>(Type implementation, EnLifestyle lifestyle)
            where TService : class;

        /// <summary> Register service collection entry </summary>
        /// <param name="service">Service type</param>
        /// <param name="implementation">Entry implementation type</param>
        /// <param name="lifestyle">Lifestyle</param>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterCollectionEntry(Type service, Type implementation, EnLifestyle lifestyle);

        /// <summary> Register service collection entry instance </summary>
        /// <param name="instance">Service collection entry instance</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IManualRegistrationsContainer</returns>
        public IManualRegistrationsContainer RegisterCollectionEntryInstance<TService>(TService instance)
            where TService : class;

        /// <summary> Register service collection entry instance </summary>
        /// <param name="service">Service type</param>
        /// <param name="instance">Service collection entry instance</param>
        /// <returns>IManualRegistrationsContainer</returns>
        public IManualRegistrationsContainer RegisterCollectionEntryInstance(Type service, object instance);
    }
}