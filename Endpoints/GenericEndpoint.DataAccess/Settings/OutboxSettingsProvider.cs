namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Settings
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Settings;

    [Component(EnLifestyle.Singleton)]
    internal class OutboxSettingsProvider : BaseSettingsProvider<OutboxSettings>,
                                            IResolvable<ISettingsProvider<OutboxSettings>>
    {
        public OutboxSettingsProvider(ConfigurationSettingsProvider<OutboxSettings> underlyingSettingProvider)
            : base(underlyingSettingProvider)
        {
        }
    }
}