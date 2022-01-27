namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Enumerations;

    /// <summary>
    /// IRegisterComponentsOverrideContainer
    /// </summary>
    public interface IRegisterComponentsOverrideContainer
    {
        /// <summary>
        /// Overrides service registration with specified replacement
        /// </summary>
        /// <param name="lifestyle">Implementation replacement lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <typeparam name="TReplacement">TReplacement type-argument</typeparam>
        /// <returns>IRegisterComponentsOverrideContainer</returns>
        IRegisterComponentsOverrideContainer Override<TService, TReplacement>(EnLifestyle lifestyle)
            where TReplacement : TService;

        /// <summary>
        /// Overrides service registration with specified replacement
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="replacement">Implementation replacement</param>
        /// <param name="lifestyle">Implementation replacement lifestyle</param>
        /// <returns>IRegisterComponentsOverrideContainer</returns>
        IRegisterComponentsOverrideContainer Override(Type service, Type replacement, EnLifestyle lifestyle);

        /// <summary>
        /// Overrides service registration with specified instance
        /// </summary>
        /// <param name="replacement">Instance override</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IRegisterComponentsOverrideContainer</returns>
        IRegisterComponentsOverrideContainer OverrideInstance<TService>(TService replacement)
            where TService : notnull;

        /// <summary>
        /// Overrides service registration with specified instance
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="replacement">Instance override</param>
        /// <returns>IRegisterComponentsOverrideContainer</returns>
        IRegisterComponentsOverrideContainer OverrideInstance(Type service, object replacement);

        /// <summary>
        /// Overrides service registration with specified factory
        /// </summary>
        /// <param name="replacement">Factory delegate override</param>
        /// <param name="lifestyle">Factory delegate replacement lifestyle</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IRegisterComponentsOverrideContainer</returns>
        IRegisterComponentsOverrideContainer OverrideDelegate<TService>(Func<TService> replacement, EnLifestyle lifestyle)
            where TService : class;

        /// <summary>
        /// Overrides service registration with specified factory
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="replacement">Factory delegate override</param>
        /// <param name="lifestyle">Factory delegate replacement lifestyle</param>
        /// <returns>IRegisterComponentsOverrideContainer</returns>
        IRegisterComponentsOverrideContainer OverrideDelegate(Type service, Func<object> replacement, EnLifestyle lifestyle);

        /// <summary>
        /// Overrides service collection registration with specified list of replacements
        /// </summary>
        /// <param name="instanceReplacements">Collection of replacement instances</param>
        /// <param name="replacements">Collection of replacement types</param>
        /// <param name="instanceProducerReplacements">Collection of replacement instance producers</param>
        /// <typeparam name="TService">TService type-argument</typeparam>
        /// <returns>IManualRegistrationsContainer</returns>
        IRegisterComponentsOverrideContainer OverrideCollection<TService>(
            IEnumerable<object> instanceReplacements,
            IEnumerable<(Type implementation, EnLifestyle lifestyle)> replacements,
            IEnumerable<(Func<TService> instanceProducer, EnLifestyle lifestyle)> instanceProducerReplacements)
            where TService : class;

        /// <summary>
        /// Overrides service collection registration with specified list of replacements
        /// </summary>
        /// <param name="service">Service type</param>
        /// <param name="instanceReplacements">Collection of replacement instances</param>
        /// <param name="replacements">Collection of replacement types</param>
        /// <param name="instanceProducerReplacements">Collection of replacement instance producers</param>
        /// <returns>IManualRegistrationsContainer</returns>
        IRegisterComponentsOverrideContainer OverrideCollection(
            Type service,
            IEnumerable<object> instanceReplacements,
            IEnumerable<(Type implementation, EnLifestyle lifestyle)> replacements,
            IEnumerable<(Func<object> instanceProducer, EnLifestyle lifestyle)> instanceProducerReplacements);
    }
}