namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class FileSystemSettingsProvider : BaseSettingsProvider<FileSystemSettings>,
                                                IResolvable<ISettingsProvider<FileSystemSettings>>
    {
        public FileSystemSettingsProvider(EnvironmentSettingsProvider<FileSystemSettings> underlyingSettingProvider)
            : base(underlyingSettingProvider)
        {
        }
    }
}