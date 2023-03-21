namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using System.IO;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;

    /// <summary>
    /// SettingsDirectoryProvider
    /// </summary>
    [ManuallyRegisteredComponent("settings directory configuration")]
    public class SettingsDirectoryProvider : IResolvable<SettingsDirectoryProvider>
    {
        /// <summary> .cctor </summary>
        /// <param name="settingsDirectory">Settings directory</param>
        public SettingsDirectoryProvider(DirectoryInfo settingsDirectory)
        {
            SettingsDirectory = settingsDirectory;
        }

        /// <summary>
        /// Settings directory
        /// </summary>
        public DirectoryInfo SettingsDirectory { get; }
    }
}