namespace SpaceEngineers.Core.CrossCuttingConcerns.Api.Extensions
{
    using System;
    using System.IO;

    /// <summary>
    /// Settings extensions
    /// </summary>
    public static class SettingsExtensions
    {
        /// <summary>
        /// File system settings directory path
        /// </summary>
        public const string FileSystemSettingsDirectory = nameof(FileSystemSettingsDirectory);

        /// <summary>
        /// Setup FileSystemSettingsDirectory
        /// </summary>
        /// <param name="settingsDirectory">Settings directory info</param>
        public static void SetupFileSystemSettingsDirectory(this DirectoryInfo settingsDirectory)
        {
            Environment.SetEnvironmentVariable(FileSystemSettingsDirectory,
                                               settingsDirectory.FullName,
                                               EnvironmentVariableTarget.Process);
        }
    }
}