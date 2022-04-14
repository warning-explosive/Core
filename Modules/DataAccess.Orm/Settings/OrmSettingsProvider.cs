namespace SpaceEngineers.Core.DataAccess.Orm.Settings
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Settings;

    [Component(EnLifestyle.Singleton)]
    internal class OrmSettingsProvider : BaseSettingsProvider<OrmSettings>,
                                         IResolvable<ISettingsProvider<OrmSettings>>
    {
        public OrmSettingsProvider(ConfigurationSettingsProvider<OrmSettings> underlyingSettingProvider)
            : base(underlyingSettingProvider)
        {
        }
    }
}