namespace SpaceEngineers.Core.SettingsManager.Internals
{
    using System;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class SettingsManagerImpl<TSettings> : ISettingsManager<TSettings>
        where TSettings : ISettings, new()
    {
        private readonly IAsyncFormatter<TSettings> _formatter;

        public SettingsManagerImpl(IAsyncFormatter<TSettings> formatter)
        {
            _formatter = formatter;
        }

        public Task<TSettings> Get()
        {
            if (typeof(IFileSystemSettings).IsAssignableFrom(typeof(TSettings)))
            {
                return _formatter.Deserialize();
            }

            throw new NotSupportedException(typeof(TSettings).FullName);
        }

        public Task Set(TSettings value)
        {
            if (typeof(IFileSystemSettings).IsAssignableFrom(typeof(TSettings)))
            {
                return _formatter.Serialize(value);
            }

            throw new NotSupportedException(typeof(TSettings).FullName);
        }
    }
}