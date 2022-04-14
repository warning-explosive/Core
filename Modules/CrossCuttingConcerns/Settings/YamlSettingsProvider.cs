namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization.TypeResolvers;

    /// <summary>
    /// YamlSettingsProvider
    /// </summary>
    /// <typeparam name="TSettings">TSettings type-argument</typeparam>
    [Component(EnLifestyle.Singleton)]
    public class YamlSettingsProvider<TSettings> : FileSystemSettingsProviderBase<TSettings>,
                                                   IResolvable<YamlSettingsProvider<TSettings>>,
                                                   ICollectionResolvable<ISettingsProvider<TSettings>>
        where TSettings : class, ISettings, new()
    {
        private readonly IDeserializer _deserializer =
            new DeserializerBuilder()
               .WithNamingConvention(PascalCaseNamingConvention.Instance)
               .WithTypeResolver(new DynamicTypeResolver())
               .Build();

        /// <summary> .cctor </summary>
        /// <param name="settingsScopeProvider">ISettingsScopeProvider</param>
        /// <param name="fileSystemSettingsProvider">FileSystemSettings provider</param>
        public YamlSettingsProvider(
            ISettingsScopeProvider settingsScopeProvider,
            ISettingsProvider<FileSystemSettings> fileSystemSettingsProvider)
            : base(settingsScopeProvider, fileSystemSettingsProvider)
        {
        }

        /// <inheritdoc />
        protected sealed override string Extension { get; } = "yaml";

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
                   .ReadAllAsync(Encoding, token)
                   .ConfigureAwait(false);

                return _deserializer.Deserialize<TSettings>(serialized);
            }
        }
    }
}