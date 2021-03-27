namespace SpaceEngineers.Core.AutoWiring.Api.Enumerations
{
    /// <summary>
    /// Component kind
    /// </summary>
    public enum EnComponentKind
    {
        /// <summary>
        /// Regular automatically registered component
        /// </summary>
        Regular,

        /// <summary>
        /// Component that should be registered as open-generic fallback
        /// </summary>
        OpenGenericFallback,

        /// <summary>
        /// Component that should be registered by hand
        /// </summary>
        ManuallyRegistered,

        /// <summary>
        /// Component that shouldn't be registered
        /// </summary>
        Unregistered,

        /// <summary>
        /// Component that should override existing registration
        /// </summary>
        Override
    }
}