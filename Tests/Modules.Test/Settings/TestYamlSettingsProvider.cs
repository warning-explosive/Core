namespace SpaceEngineers.Core.Modules.Test.Settings
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Settings;

    [Component(EnLifestyle.Singleton)]
    internal class TestYamlSettingsProvider : BaseSettingsProvider<TestYamlSettings>,
                                              IResolvable<ISettingsProvider<TestYamlSettings>>
    {
        public TestYamlSettingsProvider(YamlSettingsProvider<TestYamlSettings> underlyingSettingProvider)
            : base(underlyingSettingProvider)
        {
        }
    }
}