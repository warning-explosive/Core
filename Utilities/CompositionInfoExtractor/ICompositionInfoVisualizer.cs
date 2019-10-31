namespace SpaceEngineers.Core.Utilities.CompositionInfoExtractor
{
    using CompositionRoot;
    using CompositionRoot.Abstractions;

    /// <summary>
    /// Visualization of composition info
    /// </summary>
    public interface ICompositionInfoVisualizer : IResolvable
    {
        /// <summary>
        /// Visualize composition info
        /// </summary>
        /// <param name="compositionInfo">Composition info</param>
        /// <returns>Visualization string</returns>
        string Visualize(DependencyInfo[] compositionInfo);
    }
}