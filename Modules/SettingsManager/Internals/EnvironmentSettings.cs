namespace SpaceEngineers.Core.SettingsManager.Internals
{
    using System.Collections.Generic;
    using Abstractions;

    /// <summary>
    /// Represents all settings stored in process environment
    /// </summary>
    public sealed class EnvironmentSettings : ISettings
    {
        /// <summary> .cctor </summary>
        /// <param name="settings">EnvironmentSettingsEntries</param>
        public EnvironmentSettings(IReadOnlyCollection<EnvironmentSettingsEntry> settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// All settings stored in process environment
        /// </summary>
        public IReadOnlyCollection<EnvironmentSettingsEntry> Settings { get; }
    }
}