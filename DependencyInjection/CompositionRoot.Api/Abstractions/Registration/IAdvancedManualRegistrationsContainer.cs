namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration
{
    using System;
    using AutoRegistration.Api.Enumerations;
    using Container;

    /// <summary>
    /// IAdvancedManualRegistrationsContainer
    /// </summary>
    public interface IAdvancedManualRegistrationsContainer : IManualRegistrationsContainer
    {
        /// <summary>
        /// Dependency container implementation
        /// </summary>
        IDependencyContainerImplementation Container { get; }

        /// <summary> Registers factory delegate </summary>
        /// <param name="factory">Factory delegate</param>
        /// <param name="lifestyle">Lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterDelegate<TService>(Func<TService> factory, EnLifestyle lifestyle)
            where TService : class;

        /// <summary> Registers factory delegate </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="factory">Factory delegate</param>
        /// <param name="lifestyle">Lifestyle</param>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterDelegate(Type serviceType, Func<object> factory, EnLifestyle lifestyle);
    }
}