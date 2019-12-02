namespace SpaceEngineers.Core.SettingsManager
{
    using System;
    using CompositionRoot.Attributes;
    using CompositionRoot.Enumerations;
    using Basics;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class SettingsMangerImpl<TSettings> : ISettingsManger<TSettings>
        where TSettings : ISettings, new()
    {
        private readonly IAsyncFormatter<TSettings> _formatter;

        public SettingsMangerImpl(IAsyncFormatter<TSettings> formatter)
        {
            _formatter = formatter;
        }

        public TSettings Get()
        {
            if (typeof(TSettings).IsDerivedFromInterface(typeof(IFileSystemSettings)))
            {
                return _formatter.Deserialize().Result;
            }
            
            throw new NotSupportedException(typeof(TSettings).FullName);
        }

        public void Set(TSettings value)
        {
            if (typeof(TSettings).IsDerivedFromInterface(typeof(IFileSystemSettings)))
            {
                _formatter.Serialize(value).Wait();
                
                return;
            }
            
            throw new NotSupportedException(typeof(TSettings).FullName);
        }
    }
}