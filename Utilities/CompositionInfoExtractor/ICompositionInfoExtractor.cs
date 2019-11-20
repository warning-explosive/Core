namespace SpaceEngineers.Core.CompositionInfoExtractor
{
    using CompositionRoot;
    using CompositionRoot.Abstractions;

    /// <summary>
    /// Extracts late-bound (bound at runtime) information about project objects composition
    /// </summary>
    public interface ICompositionInfoExtractor : IResolvable
    {
        /// <summary>
        /// Extract project objects composition info
        /// </summary>
        /// <returns>DependencyInfos</returns>
        DependencyInfo[] GetCompositionInfo();
    }
}