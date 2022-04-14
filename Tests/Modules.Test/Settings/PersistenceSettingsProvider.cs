namespace SpaceEngineers.Core.Modules.Test.Settings
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Settings;

    [Component(EnLifestyle.Singleton)]
    internal class PersistenceSettingsProvider : BaseSettingsProvider<PersistenceSettings>,
                                                 IResolvable<ISettingsProvider<PersistenceSettings>>
    {
        public PersistenceSettingsProvider(JsonSettingsProvider<PersistenceSettings> underlyingSettingProvider)
            : base(underlyingSettingProvider)
        {
        }
    }
}