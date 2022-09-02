namespace SpaceEngineers.Core.CompositionRoot.CompositionInfo
{
    using System.Collections.Generic;

    /// <summary>
    /// Interpretation of composition info
    /// </summary>
    /// <typeparam name="TOutput">TOutput type-argument</typeparam>
    public interface ICompositionInfoInterpreter<out TOutput>
    {
        /// <summary>
        /// Visualize composition info
        /// </summary>
        /// <param name="compositionInfo">Composition info</param>
        /// <returns>Visualization string</returns>
        TOutput Visualize(IReadOnlyCollection<IDependencyInfo> compositionInfo);
    }
}