namespace SpaceEngineers.Core.CompositionRoot.Api.Abstractions.CompositionInfo
{
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;

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