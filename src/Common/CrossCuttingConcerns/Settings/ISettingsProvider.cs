namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using System.Diagnostics.CodeAnalysis;

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
        /// <returns>Get operation with ISettings value as result</returns>
        TSettings Get();
    }
}