namespace SpaceEngineers.Core.SettingsManager
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Extensions;

    /// <summary>
    /// Filesystem formatter
    /// </summary>
    public abstract class FileSystemFormatterBase : IAsyncFormatter
    {
        private static readonly Encoding _encoding = new UTF8Encoding(true);
        
        private static readonly string _folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Settings");
        
        /// <summary>
        /// Extension of file
        /// </summary>
        protected abstract string Extension { get; }
        
        /// <summary>
        /// Path to all filesystem settings
        /// </summary>
        protected Func<Type, string> SettingsPath => type => Path.ChangeExtension(Path.Combine(_folder, type.Name), Extension);

        /// <inheritdoc />
        public async Task Serialize<TSettings>(TSettings value)
            where TSettings : ISettings, new()
        {
            using (var fileStream = File.Open(SettingsPath(typeof(TSettings)), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                var serialized = await Task.Run(() => SerializeInternal(value));

                await fileStream.OverWriteAllAsync(serialized, _encoding);
            }
        }

        /// <inheritdoc />
        public async Task<TSettings> Deserialize<TSettings>()
            where TSettings : ISettings, new()
        {
            using (var fileStream = File.Open(SettingsPath(typeof(TSettings)), FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var serialized = await fileStream.ReadAllAsync(_encoding);
                
                return await Task.Run(() => DeserializeInternal<TSettings>(serialized));
            }
        }

        /// <summary>
        /// Serialize from ISettings object to string
        /// </summary>
        /// <param name="value">ISettings instance</param>
        /// <typeparam name="TSettings">ISettings</typeparam>
        /// <returns>Serialized string</returns>
        protected abstract string SerializeInternal<TSettings>(TSettings value)
            where TSettings : ISettings, new();

        /// <summary>
        /// Deserialize from string to ISettings object
        /// </summary>
        /// <param name="serialized">Serialized string</param>
        /// <typeparam name="TSettings">ISettings</typeparam>
        /// <returns>Deserialized ISettings object</returns>
        protected abstract TSettings DeserializeInternal<TSettings>(string serialized)
            where TSettings : ISettings, new();
    }
}