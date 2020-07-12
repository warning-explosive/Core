namespace SpaceEngineers.Core.CompositionInfoExtractor
{
    using AutoRegistration;
    using AutoWiringApi.Abstractions;

    /// <summary>
    /// Extracts late-bound (bound at runtime) information about project objects composition
    /// </summary>
    public interface ICompositionInfoExtractor : IResolvable
    {
        /// <summary>
        /// Extract project objects composition info
        /// </summary>
        /// <param name="activeMode">Get composition info in ACTIVE(TRUE) or PASSIVE(FALSE) mode (ACTIVE - try build all dependencies from AutoWiring.API; PASSIVE - Get current built components)</param>
        /// <returns>DependencyInfos</returns>
        DependencyInfo[] GetCompositionInfo(bool activeMode);
    }
}