namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IComponentsOverrideContainer
    /// </summary>
    public interface IComponentsOverrideContainer : IResolvable
    {
        /// <summary>
        /// Resolvable overrides
        /// </summary>
        IReadOnlyCollection<IRegistrationInfo> ResolvableOverrides { get; }

        /// <summary>
        /// Collection overrides
        /// </summary>
        IReadOnlyDictionary<Type, IRegistrationInfo[]> CollectionOverrides { get; }

        /// <summary>
        /// Decorator overrides
        /// </summary>
        IReadOnlyCollection<DecoratorRegistrationInfo> DecoratorOverrides { get; }
    }
}