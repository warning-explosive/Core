namespace SpaceEngineers.Core.AutoWiring.Api.Services
{
    using System.Collections.Generic;
    using Abstractions;

    /// <summary>
    /// Interpretation of composition info
    /// </summary>
    /// <typeparam name="TOutput">TOutput type-argument</typeparam>
    public interface ICompositionInfoInterpreter<out TOutput> : IResolvable
    {
        /// <summary>
        /// Visualize composition info
        /// </summary>
        /// <param name="compositionInfo">Composition info</param>
        /// <returns>Visualization string</returns>
        TOutput Visualize(IReadOnlyCollection<IDependencyInfo> compositionInfo);
    }
}