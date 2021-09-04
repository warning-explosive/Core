namespace SpaceEngineers.Core.Roslyn.Test.Extensions
{
    using System.IO;
    using System.Reflection;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// MetadataReferenceExtensions
    /// </summary>
    public static class MetadataReferenceExtensions
    {
        /// <summary> Creates MetadataReference from assembly </summary>
        /// <param name="assembly">Assembly</param>
        /// <returns>MetadataReference</returns>
        public static MetadataReference AsMetadataReference(this Assembly assembly)
        {
            return MetadataReference.CreateFromFile(assembly.Location);
        }

        /// <summary> Creates MetadataReference from assembly file location </summary>
        /// <param name="assemblyFile">Assembly file location</param>
        /// <returns>MetadataReference</returns>
        public static MetadataReference AsMetadataReference(this FileInfo assemblyFile)
        {
            return MetadataReference.CreateFromFile(assemblyFile.FullName);
        }
    }
}