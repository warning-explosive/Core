namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    /// <summary>
    /// Represents settings entry stored in process environment
    /// </summary>
    public sealed class EnvironmentSettingsEntry
    {
        /// <summary> .cctor </summary>
        /// <param name="key">Settings entry key</param>
        /// <param name="value">Settings entry value</param>
        public EnvironmentSettingsEntry(string key, string value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Settings entry key
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Settings entry value
        /// </summary>
        public string Value { get; }
    }
}