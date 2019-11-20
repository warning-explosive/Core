namespace SpaceEngineers.Core.SettingsManager
{
    using CompositionRoot.Abstractions;

    /// <summary>
    /// Settings manager
    /// </summary>
    public interface ISettingsManger : IResolvable
    {
        /// <summary>
        /// Get ISettings value
        /// </summary>
        /// <typeparam name="TSettings">ISettings</typeparam>
        /// <returns>ISettings value</returns>
        TSettings Get<TSettings>()
            where TSettings : ISettings, new();

        /// <summary>
        /// Set ISettings value
        /// </summary>
        /// <param name="value">ISettings value</param>
        /// <typeparam name="TSettings">ISettings</typeparam>
        void Set<TSettings>(TSettings value)
            where TSettings : ISettings, new();
    }
}