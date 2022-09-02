namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// IConfigurationProvider
    /// </summary>
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Tries get settings from specified path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="settings">Output settings</param>
        /// <typeparam name="TSettings">TSettings type-argument</typeparam>
        /// <returns>TryGet result</returns>
        bool TryGet<TSettings>(string path, [NotNullWhen(true)] out TSettings? settings)
            where TSettings : class, ISettings, new();
    }
}