namespace SpaceEngineers.Core.SettingsManager.Extensions
{
    using System;
    using System.IO;

    /// <summary>
    /// Settings extensions
    /// </summary>
    public static class SettingsExtensions
    {
        /// <summary>
        /// Setup FileSystemSettingsDirectory
        /// </summary>
        /// <param name="settingsDirectory">Settings directory info</param>
        public static void SetupFileSystemSettingsDirectory(this DirectoryInfo settingsDirectory)
        {
            Environment.SetEnvironmentVariable(Constants.FileSystemSettingsDirectory,
                                               settingsDirectory.FullName,
                                               EnvironmentVariableTarget.Process);
        }
    }
}