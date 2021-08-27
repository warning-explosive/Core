namespace SpaceEngineers.Core.Roslyn.Test.ValueObjects
{
    using System.Collections.Immutable;
    using Basics;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Analyzed document
    /// </summary>
    public class AnalyzedDocument
    {
        /// <summary> .cctor </summary>
        /// <param name="document">Document</param>
        /// <param name="actualDiagnostics">Actual diagnostics</param>
        public AnalyzedDocument(Document document, ImmutableArray<Diagnostic> actualDiagnostics)
        {
            Name = document.Name.NameWithoutExtension();
            Document = document;
            ActualDiagnostics = actualDiagnostics;
        }

        /// <summary>
        /// Name (without extension)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Document
        /// </summary>
        public Document Document { get; }

        /// <summary>
        /// Actual diagnostics
        /// </summary>
        public ImmutableArray<Diagnostic> ActualDiagnostics { get; }
    }
}