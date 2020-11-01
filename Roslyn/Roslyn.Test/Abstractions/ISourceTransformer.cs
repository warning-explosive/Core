namespace SpaceEngineers.Core.Roslyn.Test.Abstractions
{
    using AutoWiringApi.Abstractions;
    using Microsoft.CodeAnalysis.Text;

    /// <summary>
    /// IAliasHandler
    /// </summary>
    public interface ISourceTransformer : IResolvable
    {
        /// <summary>
        /// Transform source
        /// </summary>
        /// <param name="source">Input source</param>
        /// <returns>Transformed source</returns>
        SourceText Transform(SourceText source);
    }
}