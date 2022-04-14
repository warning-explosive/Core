namespace SpaceEngineers.Core.IntegrationTransport.Settings
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Settings;

    [Component(EnLifestyle.Singleton)]
    internal class IntegrationTransportSettingsProvider : BaseSettingsProvider<IntegrationTransportSettings>,
                                                          IResolvable<ISettingsProvider<IntegrationTransportSettings>>
    {
        public IntegrationTransportSettingsProvider(ISettingsProvider<IntegrationTransportSettings> underlyingSettingProvider)
            : base(underlyingSettingProvider)
        {
        }
    }
}