namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Settings provider
    /// </summary>
    /// <typeparam name="TSettings">ISettings</typeparam>
    [SuppressMessage("Analysis", "CA1716", Justification = "desired name")]
    public interface ISettingsProvider<TSettings>
        where TSettings : class, ISettings
    {
        /// <summary>
        /// Get ISettings value
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Get operation with ISettings value as result</returns>
        Task<TSettings> Get(CancellationToken token);

        /// <summary>
        /// Set ISettings value
        /// </summary>
        /// <param name="value">ISettings value</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Set operation</returns>
        Task Set(TSettings value, CancellationToken token);
    }
}