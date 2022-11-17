namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Json;

    /// <summary>
    /// JsonSettingsProvider
    /// </summary>
    /// <typeparam name="TSettings">TSettings type-argument</typeparam>
    [Component(EnLifestyle.Singleton)]
    public class JsonSettingsProvider<TSettings> : FileSystemSettingsProviderBase<TSettings>,
                                                   IResolvable<JsonSettingsProvider<TSettings>>,
                                                   ICollectionResolvable<ISettingsProvider<TSettings>>
        where TSettings : class, ISettings, new()
    {
        private readonly IJsonSerializer _jsonSerializer;

        /// <summary> .cctor </summary>
        /// <param name="settingsScopeProvider">ISettingsScopeProvider</param>
        /// <param name="fileSystemSettingsProvider">FileSystemSettings provider</param>
        /// <param name="jsonSerializer">IJsonSerializer</param>
        public JsonSettingsProvider(
            ISettingsScopeProvider settingsScopeProvider,
            ISettingsProvider<FileSystemSettings> fileSystemSettingsProvider,
            IJsonSerializer jsonSerializer)
            : base(settingsScopeProvider, fileSystemSettingsProvider)
        {
            _jsonSerializer = jsonSerializer;
        }

        /// <inheritdoc />
        protected sealed override string Extension { get; } = "json";

        /// <inheritdoc />
        protected sealed override string FileName { get; } = typeof(TSettings).Name;

        /// <inheritdoc />
        public sealed override async Task<TSettings> Get(CancellationToken token)
        {
            var scoped = GetSettingsFileInfo(Extension, FileSystemSettingsDirectory, FileSystemSettingsScope ?? string.Empty, FileName);
            var common = GetSettingsFileInfo(Extension, FileSystemSettingsDirectory, FileName);

            return scoped.Exists
                ? await ReadAndDeserialize(scoped, token).ConfigureAwait(false)
                : common.Exists
                    ? await ReadAndDeserialize(common, token).ConfigureAwait(false)
                    : new TSettings();
        }

        private async Task<TSettings> ReadAndDeserialize(FileInfo file, CancellationToken token)
        {
            using (var fileStream = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var serialized = await fileStream
                   .AsString(Encoding, token)
                   .ConfigureAwait(false);

                return _jsonSerializer.DeserializeObject<TSettings>(serialized);
            }
        }
    }
}