namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    /// <summary>
    /// DefaultSettingsScopeProvider
    /// </summary>
    [Component(EnLifestyle.Singleton)]
    public class DefaultSettingsScopeProvider : ISettingsScopeProvider
    {
        /// <inheritdoc />
        public string? Scope => default;
    }
}