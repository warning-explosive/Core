namespace SpaceEngineers.Core.SettingsManager.Internals
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Abstractions;
    using Basics;

    internal abstract class FileSystemSettingsManagerBase<TSettings> : ISettingsManager<TSettings>
        where TSettings : class, IFileSystemSettings
    {
        private readonly Encoding _encoding = new UTF8Encoding(true);
        private readonly string _folder;

        protected FileSystemSettingsManagerBase()
        {
            var dir = new EnvironmentSettingsManager().Get(Constants.FileSystemSettingsDirectory).Value;

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
        public async Task Set(TSettings value)
        {
            using (var fileStream = File.Open(SettingsPath(typeof(TSettings)), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                var serialized = SerializeInternal(value);

                await fileStream.OverWriteAllAsync(serialized, _encoding).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async Task<TSettings> Get()
        {
            using (var fileStream = File.Open(SettingsPath(typeof(TSettings)), FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var serialized = await fileStream.ReadAllAsync(_encoding).ConfigureAwait(false);

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