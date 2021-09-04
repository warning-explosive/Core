namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration
{
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IComponentsOverrideContainer
    /// </summary>
    public interface IComponentsOverrideContainer : IResolvable
    {
        /// <summary>
        /// Instance overrides
        /// </summary>
        IReadOnlyCollection<InstanceRegistrationInfo> InstanceOverrides { get; }

        /// <summary>
        /// Delegate overrides
        /// </summary>
        IReadOnlyCollection<DelegateRegistrationInfo> DelegateOverrides { get; }

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