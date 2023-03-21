namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using AutoRegistration.Api.Enumerations;

    /// <summary>
    /// IAdvancedManualRegistrationsContainer
    /// </summary>
    public interface IAdvancedManualRegistrationsContainer : IManualRegistrationsContainer
    {
        /// <summary>
        /// Dependency container implementation
        /// </summary>
        IDependencyContainer Container { get; }

        /// <summary> Registers instance producer </summary>
        /// <param name="instanceProducer">Instance producer</param>
        /// <param name="lifestyle">Lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterDelegate<TService>(Func<TService> instanceProducer, EnLifestyle lifestyle)
            where TService : class;

        /// <summary> Registers instance producer </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="instanceProducer">Instance producer</param>
        /// <param name="lifestyle">Lifestyle</param>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterDelegate(Type serviceType, Func<object> instanceProducer, EnLifestyle lifestyle);

        /// <summary> Register service collection entry instance producer </summary>
        /// <param name="instanceProducer">Instance producer</param>
        /// <param name="lifestyle">Lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IManualRegistrationsContainer</returns>
        public IManualRegistrationsContainer RegisterCollectionEntryDelegate<TService>(Func<TService> instanceProducer, EnLifestyle lifestyle)
            where TService : class;

        /// <summary> Register service collection entry instance producer </summary>
        /// <param name="service">Service type</param>
        /// <param name="instanceProducer">Instance producer</param>
        /// <param name="lifestyle">Lifestyle</param>
        /// <returns>IManualRegistrationsContainer</returns>
        public IManualRegistrationsContainer RegisterCollectionEntryDelegate(Type service, Func<object> instanceProducer, EnLifestyle lifestyle);
    }
}