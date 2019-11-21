namespace SpaceEngineers.Core.SettingsManager
{
    using CompositionRoot.Abstractions;

    /// <summary>
    /// Formatter for *.yaml, *.yml files
    /// </summary>
    public interface IYamlFormatter : IAsyncFormatter, IResolvable
    {
    }
}