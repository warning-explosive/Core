namespace SpaceEngineers.Core.Modules.Test.Settings
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Settings;

    [Component(EnLifestyle.Singleton)]
    internal class TestConfigurationSettingsProvider : BaseSettingsProvider<TestConfigurationSettings>,
                                                       IResolvable<ISettingsProvider<TestConfigurationSettings>>
    {
        public TestConfigurationSettingsProvider(ConfigurationSettingsProvider<TestConfigurationSettings> underlyingSettingProvider)
            : base(underlyingSettingProvider)
        {
        }
    }
}