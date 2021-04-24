namespace SpaceEngineers.Core.AutoWiring.Api.Enumerations
{
    /// <summary>
    /// Component registration kind
    /// </summary>
    public enum EnComponentRegistrationKind
    {
        /// <summary>
        /// Component that should be registered automatically
        /// </summary>
        AutomaticallyRegistered,

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