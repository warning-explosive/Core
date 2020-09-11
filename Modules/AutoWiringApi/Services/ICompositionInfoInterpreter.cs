namespace SpaceEngineers.Core.AutoWiringApi.Services
{
    using System.Collections.Generic;
    using Abstractions;
    using Contexts;

    /// <summary>
    /// Interpretation of composition info
    /// </summary>
    /// <typeparam name="TOutput">TOutput type-argument</typeparam>
    public interface ICompositionInfoInterpreter<TOutput> : IResolvable
    {
        /// <summary>
        /// Visualize composition info
        /// </summary>
        /// <param name="compositionInfo">Composition info</param>
        /// <returns>Visualization string</returns>
        TOutput Visualize(IReadOnlyCollection<IDependencyInfo> compositionInfo);
    }
}