namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions
{
    using System;
    using AutoRegistration.Api.Enumerations;

    /// <summary>
    /// IComponentsOverrideContainer
    /// </summary>
    public interface IComponentsOverrideContainer
    {
        /// <summary>
        /// Overrides exact component
        /// </summary>
        /// <param name="lifestyle">Implementation replacement lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <typeparam name="TImplementation">TImplementation type-argument</typeparam>
        /// <typeparam name="TReplacement">TReplacement type-argument</typeparam>
        /// <returns>IComponentsOverrideContainer</returns>
        IComponentsOverrideContainer Override<TService, TImplementation, TReplacement>(EnLifestyle lifestyle)
            where TImplementation : TService
            where TReplacement : TService;

        /// <summary>
        /// Overrides exact component
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="implementation">Implementation</param>
        /// <param name="replacement">Implementation replacement</param>
        /// <param name="lifestyle">Implementation replacement lifestyle</param>
        /// <returns>IComponentsOverrideContainer</returns>
        IComponentsOverrideContainer Override(Type service, Type implementation, Type replacement, EnLifestyle lifestyle);
    }
}