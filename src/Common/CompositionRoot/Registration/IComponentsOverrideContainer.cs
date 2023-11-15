namespace SpaceEngineers.Core.CompositionRoot.Registration
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// IComponentsOverrideContainer
    /// </summary>
    public interface IComponentsOverrideContainer
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