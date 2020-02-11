namespace SpaceEngineers.Core.SettingsManager
{
    using System;
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

        public TSettings Get()
        {
            if (typeof(IFileSystemSettings).IsAssignableFrom(typeof(TSettings)))
            {
                return _formatter.Deserialize().Result;
            }

            throw new NotSupportedException(typeof(TSettings).FullName);
        }

        public void Set(TSettings value)
        {
            if (typeof(IFileSystemSettings).IsAssignableFrom(typeof(TSettings)))
            {
                _formatter.Serialize(value).Wait();

                return;
            }

            throw new NotSupportedException(typeof(TSettings).FullName);
        }
    }
}