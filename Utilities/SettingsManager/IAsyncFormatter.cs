namespace SpaceEngineers.Core.SettingsManager
{
    using System.Threading.Tasks;

    /// <summary>
    /// Asynchronous formatter
    /// </summary>
    public interface IAsyncFormatter
    {
        /// <summary>
        /// Deserialize from string to ISettings object
        /// </summary>
        /// <typeparam name="TSettings">ISettings</typeparam>
        /// <returns>Deserialized ISettings</returns>
        Task<TSettings> Deserialize<TSettings>()
            where TSettings : ISettings, new();

        /// <summary>
        /// Serialize from ISettings object to string
        /// </summary>
        /// <param name="value">ISettings instance</param>
        /// <typeparam name="TSettings">ISettings</typeparam>
        /// <returns>Serialized string</returns>
        Task Serialize<TSettings>(TSettings value)
            where TSettings : ISettings, new();
    }
}