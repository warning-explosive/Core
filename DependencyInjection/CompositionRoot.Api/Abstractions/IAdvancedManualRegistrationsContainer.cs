namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions
{
    using System;
    using AutoRegistration.Api.Enumerations;

    /// <summary>
    /// IAdvancedManualRegistrationsContainer
    /// </summary>
    public interface IAdvancedManualRegistrationsContainer : IManualRegistrationsContainer
    {
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