namespace SpaceEngineers.Core.SettingsManager
{
    using System.Threading.Tasks;
    using AutoWiringApi.Abstractions;

    /// <summary>
    /// Asynchronous formatter
    /// </summary>
    /// <typeparam name="TSettings">ISettings</typeparam>
    public interface IAsyncFormatter<TSettings> : IResolvable
        where TSettings : ISettings, new()
    {
        /// <summary>
        /// Deserialize from string to ISettings object
        /// </summary>
        /// <returns>Deserialized ISettings</returns>
        Task<TSettings> Deserialize();

        /// <summary>
        /// Serialize from ISettings object to string
        /// </summary>
        /// <param name="value">ISettings instance</param>
        /// <returns>Serialized string</returns>
        Task Serialize(TSettings value);
    }
}