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

        /// <summary>
        /// Overrides instance component registered as specified service
        /// </summary>
        /// <param name="replacement">Instance override</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IComponentsOverrideContainer</returns>
        IComponentsOverrideContainer OverrideInstance<TService>(TService replacement)
            where TService : notnull;

        /// <summary>
        /// Overrides instance component registered as specified service
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="replacement">Instance override</param>
        /// <returns>IComponentsOverrideContainer</returns>
        IComponentsOverrideContainer OverrideInstance(Type service, object replacement);

        /// <summary>
        /// Overrides factory delegate registered as specified service
        /// </summary>
        /// <param name="replacement">Factory delegate override</param>
        /// <param name="lifestyle">Factory delegate replacement lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IComponentsOverrideContainer</returns>
        IComponentsOverrideContainer OverrideDelegate<TService>(Func<TService> replacement, EnLifestyle lifestyle)
            where TService : class;

        /// <summary>
        /// Overrides factory delegate registered as specified service
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="replacement">Factory delegate override</param>
        /// <param name="lifestyle">Factory delegate replacement lifestyle</param>
        /// <returns>IComponentsOverrideContainer</returns>
        IComponentsOverrideContainer OverrideDelegate(Type service, Func<object> replacement, EnLifestyle lifestyle);
    }
}