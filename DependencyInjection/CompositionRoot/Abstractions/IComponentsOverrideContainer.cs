namespace SpaceEngineers.Core.CompositionRoot.Abstractions
{
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;
    using Registration;

    /// <summary>
    /// IComponentsOverrideContainer
    /// </summary>
    public interface IComponentsOverrideContainer : IResolvable
    {
        /// <summary>
        /// All overrides
        /// </summary>
        IEnumerable<ComponentOverrideInfo> AllOverrides { get; }

        /// <summary>
        /// Resolvable overrides
        /// </summary>
        IReadOnlyCollection<ComponentOverrideInfo> ResolvableOverrides { get; }

        /// <summary>
        /// Collection resolvable overrides
        /// </summary>
        IReadOnlyCollection<ComponentOverrideInfo> CollectionResolvableOverrides { get; }

        /// <summary>
        /// Decorator overrides
        /// </summary>
        IReadOnlyCollection<ComponentOverrideInfo> DecoratorOverrides { get; }
    }
}