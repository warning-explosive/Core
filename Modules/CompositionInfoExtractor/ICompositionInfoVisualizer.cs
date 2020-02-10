namespace SpaceEngineers.Core.CompositionInfoExtractor
{
    using AutoRegistration;
    using AutoWiringApi.Abstractions;

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