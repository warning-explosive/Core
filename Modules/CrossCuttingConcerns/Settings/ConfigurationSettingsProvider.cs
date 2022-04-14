namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    /// <summary>
    /// ConfigurationSettingsProvider
    /// </summary>
    /// <typeparam name="TSettings">TSettings type-argument</typeparam>
    [Component(EnLifestyle.Singleton)]
    public class ConfigurationSettingsProvider<TSettings> : FileSystemSettingsProviderBase<TSettings>,
                                                            IResolvable<ConfigurationSettingsProvider<TSettings>>,
                                                            ICollectionResolvable<ISettingsProvider<TSettings>>
        where TSettings : class, ISettings, new()
    {
        private readonly IConfigurationProvider _configurationProvider;

        /// <summary> .cctor </summary>
        /// <param name="settingsScopeProvider">ISettingsScopeProvider</param>
        /// <param name="fileSystemSettingsProvider">FileSystemSettings provider</param>
        /// <param name="configurationProvider">IConfigurationProvider</param>
        public ConfigurationSettingsProvider(
            ISettingsScopeProvider settingsScopeProvider,
            ISettingsProvider<FileSystemSettings> fileSystemSettingsProvider,
            IConfigurationProvider configurationProvider)
            : base(settingsScopeProvider, fileSystemSettingsProvider)
        {
            _configurationProvider = configurationProvider;
        }

        /// <inheritdoc />
        protected sealed override string FileName { get; } = "appsettings";

        /// <inheritdoc />
        protected sealed override string Extension { get; } = "json";

        /// <inheritdoc />
        public sealed override Task<TSettings> Get(CancellationToken token)
        {
            var scoped = GetSettingsFileInfo(Extension, FileSystemSettingsDirectory, FileSystemSettingsScope ?? string.Empty, FileName);

            if (scoped.Exists && _configurationProvider.TryGet<TSettings>(scoped.FullName, out var settings))
            {
                return Task.FromResult(settings);
            }

            var common = GetSettingsFileInfo(Extension, FileSystemSettingsDirectory, FileName);

            if (common.Exists && _configurationProvider.TryGet<TSettings>(common.FullName, out settings))
            {
                return Task.FromResult(settings);
            }

            return Task.FromResult(new TSettings());
        }
    }
}