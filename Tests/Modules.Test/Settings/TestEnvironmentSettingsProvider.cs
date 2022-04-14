namespace SpaceEngineers.Core.Modules.Test.Settings
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Settings;

    [Component(EnLifestyle.Singleton)]
    internal class TestEnvironmentSettingsProvider : BaseSettingsProvider<TestEnvironmentSettings>,
                                                     IResolvable<ISettingsProvider<TestEnvironmentSettings>>
    {
        public TestEnvironmentSettingsProvider(EnvironmentSettingsProvider<TestEnvironmentSettings> underlyingSettingProvider)
            : base(underlyingSettingProvider)
        {
        }
    }
}