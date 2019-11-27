namespace SpaceEngineers.Core.SettingsManager
{
    using CompositionRoot.Abstractions;

    /// <summary>
    /// Settings manager
    /// </summary>
    /// <typeparam name="TSettings">ISettings</typeparam>
    public interface ISettingsManger<TSettings> : IResolvable
        where TSettings : ISettings, new()
    {
        /// <summary>
        /// Get ISettings value
        /// </summary>
        /// <returns>ISettings value</returns>
        TSettings Get();

        /// <summary>
        /// Set ISettings value
        /// </summary>
        /// <param name="value">ISettings value</param>
        void Set(TSettings value);
    }
}