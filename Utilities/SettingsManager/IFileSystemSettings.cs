namespace SpaceEngineers.Core.SettingsManager
{
    using CompositionRoot.Abstractions;

    /// <summary>
    /// Configuration stored into machine file system
    /// </summary>
    public interface IFileSystemSettings<TFormatter> : ISettings
        where TFormatter : IAsyncFormatter, IResolvable
    {
    }
}