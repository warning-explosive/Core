namespace SpaceEngineers.Core.Modules.Test.Settings
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Settings;

    [Component(EnLifestyle.Singleton)]
    internal class TestPersistenceSettingsProvider : BaseSettingsProvider<TestPersistenceSettings>,
                                                     IResolvable<ISettingsProvider<TestPersistenceSettings>>
    {
        public TestPersistenceSettingsProvider(JsonSettingsProvider<TestPersistenceSettings> underlyingSettingProvider)
            : base(underlyingSettingProvider)
        {
        }
    }
}