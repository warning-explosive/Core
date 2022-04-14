namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Settings
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Settings;

    [Component(EnLifestyle.Singleton)]
    internal class SqlDatabaseSettingsProvider : BaseSettingsProvider<SqlDatabaseSettings>,
                                                 IResolvable<ISettingsProvider<SqlDatabaseSettings>>
    {
        public SqlDatabaseSettingsProvider(ConfigurationSettingsProvider<SqlDatabaseSettings> underlyingSettingProvider)
            : base(underlyingSettingProvider)
        {
        }
    }
}