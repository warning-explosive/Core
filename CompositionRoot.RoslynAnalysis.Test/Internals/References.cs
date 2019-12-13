namespace SpaceEngineers.Core.CompositionRoot.RoslynAnalysis.Test.Internals
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    internal class References
    {
        internal static MetadataReference CorlibReference { get; } =
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

        internal static MetadataReference SystemCoreReference { get; } =
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);

        internal static MetadataReference CSharpSymbolsReference { get; } =
            MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);

        internal static MetadataReference CodeAnalysisReference { get; } =
            MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);
    }
}