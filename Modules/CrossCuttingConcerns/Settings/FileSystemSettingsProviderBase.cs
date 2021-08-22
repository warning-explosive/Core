namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Basics;

    internal abstract class FileSystemSettingsProviderBase<TSettings> : ISettingsProvider<TSettings>
        where TSettings : class, IFileSystemSettings
    {
        private readonly Encoding _encoding = new UTF8Encoding(true);
        private readonly string _folder;

        protected FileSystemSettingsProviderBase()
        {
            var dir = EnvironmentSettingsProvider.Get(SettingsExtensions.FileSystemSettingsDirectory).Value;

            if (!Directory.Exists(dir))
            {
                throw new InvalidOperationException($"FileSystemSettingsDirectory: {dir} not exists");
            }

            _folder = dir;
        }

        /// <summary>
        /// Extension of file
        /// </summary>
        protected abstract string Extension { get; }

        /// <summary>
        /// Path to all filesystem settings
        /// </summary>
        private Func<Type, string> SettingsPath => type => Path.ChangeExtension(Path.Combine(_folder, type.Name), Extension);

        /// <inheritdoc />
        public async Task Set(TSettings value, CancellationToken token)
        {
            using (var fileStream = File.Open(SettingsPath(typeof(TSettings)), FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            {
                var serialized = SerializeInternal(value);

                await fileStream.OverWriteAllAsync(serialized, _encoding, token).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async Task<TSettings> Get(CancellationToken token)
        {
            using (var fileStream = File.Open(SettingsPath(typeof(TSettings)), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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
    }
}