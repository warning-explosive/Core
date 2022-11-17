namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// FileSystemSettingsProviderBase
    /// </summary>
    /// <typeparam name="TSettings">TSettings type-argument</typeparam>
    public abstract class FileSystemSettingsProviderBase<TSettings> : ISettingsProvider<TSettings>
        where TSettings : class, ISettings, new()
    {
        /// <summary> .cctor </summary>
        /// <param name="settingsScopeProvider">ISettingsScopeProvider</param>
        /// <param name="fileSystemSettingsProvider">FileSystemSettings provider</param>
        protected FileSystemSettingsProviderBase(
            ISettingsScopeProvider settingsScopeProvider,
            ISettingsProvider<FileSystemSettings> fileSystemSettingsProvider)
        {
            var directory = fileSystemSettingsProvider
               .Get(CancellationToken.None)
               .Result
               .FileSystemSettingsDirectory;

            if (!Directory.Exists(directory))
            {
                throw new InvalidOperationException($"FileSystemSettingsDirectory: {directory} not exists");
            }

            FileSystemSettingsDirectory = directory;
            FileSystemSettingsScope = settingsScopeProvider.Scope;

            Encoding = Encoding.UTF8;
        }

        /// <summary>
        /// File system settings directory
        /// </summary>
        protected string FileSystemSettingsDirectory { get; }

        /// <summary>
        /// File system settings scope
        /// </summary>
        protected string? FileSystemSettingsScope { get; }

        /// <summary>
        /// Settings file name
        /// </summary>
        protected abstract string FileName { get; }

        /// <summary>
        /// Settings file extension
        /// </summary>
        protected abstract string Extension { get; }

        /// <summary>
        /// Encoding
        /// </summary>
        protected Encoding Encoding { get; }

        /// <inheritdoc />
        [SuppressMessage("Analysis", "CA1716", Justification = "desired name")]
        public abstract Task<TSettings> Get(CancellationToken token);

        /// <summary>
        /// Gets settings file info
        /// </summary>
        /// <param name="extension">Extension</param>
        /// <param name="paths">Paths</param>
        /// <returns>Settings file info</returns>
        protected FileInfo GetSettingsFileInfo(string extension, params string[] paths)
        {
            return new FileInfo(Path.ChangeExtension(Path.Combine(paths), extension));
        }
    }
}