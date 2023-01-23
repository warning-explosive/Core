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
        /// Setup settings directory
        /// </summary>
        /// <param name="settingsDirectory">Settings directory info</param>
        public static void SetupSettingsDirectory(this DirectoryInfo settingsDirectory)
        {
            Environment.SetEnvironmentVariable(
                nameof(FileSystemSettings),
                $@"{{ ""{nameof(FileSystemSettings.SettingsDirectory)}"": ""{EscapeSpecialCharacters(settingsDirectory.FullName)}"" }}",
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