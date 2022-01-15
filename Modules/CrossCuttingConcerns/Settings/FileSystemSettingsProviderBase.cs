namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Api.Extensions;
    using Basics;

    internal abstract class FileSystemSettingsProviderBase<TSettings> : ISettingsProvider<TSettings>
        where TSettings : class, IFileSystemSettings
    {
        private readonly ISettingsScopeProvider _settingsScopeProvider;

        private readonly Encoding _encoding = new UTF8Encoding(true);
        private readonly string _folder;

        protected FileSystemSettingsProviderBase(ISettingsScopeProvider settingsScopeProvider)
        {
            _settingsScopeProvider = settingsScopeProvider;

            var dir = EnvironmentSettingsProvider.Get(SettingsExtensions.FileSystemSettingsDirectory).Value;

            if (!Directory.Exists(dir))
            {
                throw new InvalidOperationException($"FileSystemSettingsDirectory: {dir} not exists");
            }

            _folder = dir;
        }

        /// <summary>
        /// Settings file extension
        /// </summary>
        protected abstract string Extension { get; }

        /// <inheritdoc />
        public async Task Set(TSettings value, CancellationToken token)
        {
            using (var fileStream = File.Open(GetSettingsPath(typeof(TSettings)), FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            {
                var serialized = SerializeInternal(value);

                await fileStream.OverWriteAllAsync(serialized, _encoding, token).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async Task<TSettings> Get(CancellationToken token)
        {
            using (var fileStream = File.Open(GetSettingsPath(typeof(TSettings)), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var serialized = await fileStream.ReadAllAsync(_encoding, token).ConfigureAwait(false);

                return DeserializeInternal(serialized);
            }
        }

        /// <summary>
        /// Serialize from ISettings object to string
        /// </summary>
        /// <param name="value">ISettings instance</param>
        /// <returns>Serialized string</returns>
        protected abstract string SerializeInternal(TSettings value);

        /// <summary>
        /// Deserialize from string to ISettings object
        /// </summary>
        /// <param name="serialized">Serialized string</param>
        /// <returns>Deserialized ISettings object</returns>
        protected abstract TSettings DeserializeInternal(string serialized);

        private string GetSettingsPath(Type type)
        {
            var scope = _settingsScopeProvider.Scope;

            if (scope != null && !scope.IsNullOrEmpty())
            {
                var exclusivePath = Path.ChangeExtension(Path.Combine(_folder, scope, type.Name), Extension);

                if (File.Exists(exclusivePath))
                {
                    return exclusivePath;
                }
            }

            var commonPath = Path.ChangeExtension(Path.Combine(_folder, type.Name), Extension);

            return commonPath;
        }
    }
}