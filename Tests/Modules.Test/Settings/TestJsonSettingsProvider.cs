namespace SpaceEngineers.Core.Modules.Test.Settings
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Settings;

    [Component(EnLifestyle.Singleton)]
    internal class TestJsonSettingsProvider : BaseSettingsProvider<TestJsonSettings>,
                                              IResolvable<ISettingsProvider<TestJsonSettings>>
    {
        public TestJsonSettingsProvider(JsonSettingsProvider<TestJsonSettings> underlyingSettingProvider)
            : base(underlyingSettingProvider)
        {
        }
    }
}