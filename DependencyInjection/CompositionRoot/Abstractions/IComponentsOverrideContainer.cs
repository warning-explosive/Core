namespace SpaceEngineers.Core.CompositionRoot.Abstractions
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Registration;

    /// <summary>
    /// IComponentsOverrideContainer
    /// </summary>
    public interface IComponentsOverrideContainer
    {
        /// <summary>
        /// Overrides
        /// </summary>
        [SuppressMessage("Analysis", "CA1716", Justification = "desired name")]
        IReadOnlyCollection<ComponentOverrideInfo> Overrides { get; }
    }
}