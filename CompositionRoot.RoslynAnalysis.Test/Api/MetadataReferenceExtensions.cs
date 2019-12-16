namespace SpaceEngineers.Core.CompositionRoot.RoslynAnalysis.Test.Api
{
    using System.Reflection;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// MetadataReferenceExtensions
    /// </summary>
    public static class MetadataReferenceExtensions
    {
        /// <summary>
        /// Create MetadataReference for assembly
        /// </summary>
        /// <param name="assembly">Assembly</param>
        /// <returns>MetadataReference</returns>
        public static MetadataReference CreateReference(this Assembly assembly)
        {
            return MetadataReference.CreateFromFile(assembly.Location);
        }
    }
}