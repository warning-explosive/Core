namespace SpaceEngineers.Core.AuthEndpoint.Settings
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.CrossCuttingConcerns.Settings;

    [Component(EnLifestyle.Singleton)]
    internal class AuthorizationSettingsProvider : BaseSettingsProvider<AuthorizationSettings>,
                                                   IResolvable<ISettingsProvider<AuthorizationSettings>>
    {
        public AuthorizationSettingsProvider(ConfigurationSettingsProvider<AuthorizationSettings> underlyingSettingProvider)
            : base(underlyingSettingProvider)
        {
        }
    }
}