namespace SpaceEngineers.Core.Utilities.SettingsManager
{
    using System.Threading.Tasks;

    public interface IAsyncFormatter
    {
        Task<TSettings> Deserialize<TSettings>()
            where TSettings : ISettings, new();

        Task Serialize<TSettings>(TSettings value)
            where TSettings : ISettings, new();
    }
}