namespace SpaceEngineers.Core.Utilities.SettingsManager
{
    using CompositionRoot.Abstractions;

    public interface ISettingsManger : IResolvable
    {
        TSettings Get<TSettings>()
            where TSettings : ISettings, new();

        void Set<TSettings>(TSettings value)
            where TSettings : ISettings, new();
    }
}