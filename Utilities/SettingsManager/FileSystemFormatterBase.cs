namespace SpaceEngineers.Core.Utilities.SettingsManager
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using CompositionRoot.Extensions;

    public abstract class FileSystemFormatterBase : IAsyncFormatter
    {
        private static readonly Encoding _encoding = new UTF8Encoding(true);
        
        private static readonly string _folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Settings");
        
        protected abstract string Extension { get; }
        
        protected Func<Type, string> SettingsPath => type => Path.ChangeExtension(Path.Combine(_folder, type.Name), Extension);

        public async Task Serialize<TSettings>(TSettings value)
            where TSettings : ISettings, new()
        {
            using (var fileStream = File.Open(SettingsPath(typeof(TSettings)), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                var serialized = await Task.Run(() => SerializeInternal(value));

                await fileStream.OverWriteAllAsync(serialized, _encoding);
            }
        }

        public async Task<TSettings> Deserialize<TSettings>()
            where TSettings : ISettings, new()
        {
            using (var fileStream = File.Open(SettingsPath(typeof(TSettings)), FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var serialized = await fileStream.ReadAllAsync(_encoding);
                
                return await Task.Run(() => DeserializeInternal<TSettings>(serialized));
            }
        }

        protected abstract string SerializeInternal<TSettings>(TSettings value)
            where TSettings : ISettings, new();

        protected abstract TSettings DeserializeInternal<TSettings>(string serialized)
            where TSettings : ISettings, new();
    }
}