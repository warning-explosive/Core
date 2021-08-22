namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions
{
    using System;
    using AutoRegistration.Api.Enumerations;

    /// <summary>
    /// Advanced manual registrations container abstraction
    /// </summary>
    public interface IAdvancedManualRegistrationsContainer : IManualRegistrationsContainer
    {
        /// <summary> Register factory delegate </summary>
        /// <param name="factory">Factory delegate</param>
        /// <param name="lifestyle">Lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterFactory<TService>(Func<TService> factory, EnLifestyle lifestyle)
            where TService : class;

        /// <summary> Register factory delegate </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="factory">Factory delegate</param>
        /// <param name="lifestyle">Lifestyle</param>
        /// <returns>IManualRegistrationsContainer</returns>
        IManualRegistrationsContainer RegisterFactory(Type serviceType, Func<object> factory, EnLifestyle lifestyle);
    }
}