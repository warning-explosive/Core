namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
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
            Environment.SetEnvironmentVariable(
                nameof(FileSystemSettings),
                $@"{{ ""{nameof(FileSystemSettings.FileSystemSettingsDirectory)}"": ""{EscapeSpecialCharacters(settingsDirectory.FullName)}"" }}",
                EnvironmentVariableTarget.Process);

            static string EscapeSpecialCharacters(string source)
            {
                return source
                    .Replace("\\", "\\\\", StringComparison.Ordinal)
                    .Replace("\"", "\\\"", StringComparison.Ordinal);
            }
        }
    }
}