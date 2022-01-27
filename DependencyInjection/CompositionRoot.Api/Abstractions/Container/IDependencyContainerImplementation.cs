namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Container
{
    using System;
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

        /// <summary> Register dependency </summary>
        /// <param name="service">Service</param>
        /// <param name="implementation">Implementation</param>
        /// <param name="lifestyle">Lifestyle</param>
        void Register(Type service, Type implementation, EnLifestyle lifestyle);

        /// <summary> Register singleton instance </summary>
        /// <param name="service">Service</param>
        /// <param name="instance">Instance</param>
        void RegisterInstance(Type service, object instance);

        /// <summary> Register delegate </summary>
        /// <param name="service">Service</param>
        /// <param name="instanceProducer">Instance producer</param>
        /// <param name="lifestyle">Lifestyle</param>
        void RegisterDelegate(Type service, Func<object> instanceProducer, EnLifestyle lifestyle);

        /// <summary> Register open-generic fallback dependency  </summary>
        /// <param name="service">Service</param>
        /// <param name="implementation">Implementation</param>
        /// <param name="lifestyle">Lifestyle</param>
        void RegisterOpenGenericFallBack(Type service, Type implementation, EnLifestyle lifestyle);

        /// <summary> Register collection entry </summary>
        /// <param name="service">Service</param>
        /// <param name="implementation">Implementation</param>
        /// <param name="lifestyle">Lifestyle</param>
        void RegisterCollectionEntry(Type service, Type implementation, EnLifestyle lifestyle);

        /// <summary> Register collection entry instance </summary>
        /// <param name="service">Service</param>
        /// <param name="collectionEntryInstance">Collection entry instance</param>
        void RegisterCollectionEntryInstance(Type service, object collectionEntryInstance);

        /// <summary> Register collection entry delegate </summary>
        /// <param name="service">Service</param>
        /// <param name="instanceProducer">Collection entry instance producer</param>
        /// <param name="lifestyle">Lifestyle</param>
        void RegisterCollectionEntryDelegate(Type service, Func<object> instanceProducer, EnLifestyle lifestyle);

        /// <summary> Register decorator </summary>
        /// <param name="service">Service</param>
        /// <param name="implementation">Implementation</param>
        /// <param name="lifestyle">Lifestyle</param>
        void RegisterDecorator(Type service, Type implementation, EnLifestyle lifestyle);
    }
}