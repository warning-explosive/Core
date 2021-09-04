namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Container
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Enumerations;

    /// <summary>
    /// IDependencyContainerImplementation
    /// </summary>
    public interface IDependencyContainerImplementation : IDependencyContainer,
                                                          IResolvable,
                                                          IDisposable
    {
        /// <summary>
        /// Verify container
        /// </summary>
        void Verify();

        /// <summary>
        /// Register dependency
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="implementation">Implementation</param>
        /// <param name="lifestyle">Lifestyle</param>
        void Register(Type service, Type implementation, EnLifestyle lifestyle);

        /// <summary>
        /// Register factory
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="instanceProducer">Instance producer</param>
        /// <param name="lifestyle">Lifestyle</param>
        void Register(Type service, Func<object> instanceProducer, EnLifestyle lifestyle);

        /// <summary>
        /// Register singleton instance
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="instance">Instance</param>
        void RegisterInstance(Type service, object instance);

        /// <summary>
        /// Register open-generic fallback dependency
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="implementation">Implementation</param>
        /// <param name="lifestyle">Lifestyle</param>
        void RegisterOpenGenericFallBack(Type service, Type implementation, EnLifestyle lifestyle);

        /// <summary>
        /// Register collection
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="implementations">Implementations</param>
        /// <param name="lifestyle">Lifestyle</param>
        void RegisterCollection(Type service, IEnumerable<Type> implementations, EnLifestyle lifestyle);

        /// <summary>
        /// Register decorator
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="implementation">Implementation</param>
        /// <param name="lifestyle">Lifestyle</param>
        void RegisterDecorator(Type service, Type implementation, EnLifestyle lifestyle);
    }
}