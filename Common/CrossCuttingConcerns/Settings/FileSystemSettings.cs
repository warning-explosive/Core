namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using System;

    /// <summary>
    /// FileSystemSettings
    /// </summary>
    public class FileSystemSettings : ISettings
    {
        /// <summary> .cctor </summary>
        public FileSystemSettings()
        {
            SettingsDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// File system settings directory
        /// </summary>
        public string SettingsDirectory { get; set; }
    }
}