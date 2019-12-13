namespace SpaceEngineers.Core.CompositionRoot.RoslynAnalysis.Test.Internals
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Api;
    using Attributes;
    using Enumerations;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class DiagnosticAnalyzerExtractorImpl : IDiagnosticAnalyzerExtractor
    {
        private readonly IDocumentsGenerator _documentsGenerator;

        /// <summary> .ctor </summary>
        /// <param name="documentsGenerator">IDocumentsGenerator</param>
        public DiagnosticAnalyzerExtractorImpl(IDocumentsGenerator documentsGenerator)
        {
            _documentsGenerator = documentsGenerator;
        }

        /// <inheritdoc />
        public Diagnostic[] ExtractDiagnostics(string source, DiagnosticAnalyzer analyzer)
        {
            return ExecuteInternal(_documentsGenerator.CreateDocuments(new[] { source }), analyzer);
        }

        /// <inheritdoc />
        public Diagnostic[] ExtractDiagnostics(string[] sources, DiagnosticAnalyzer analyzer)
        {
            return ExecuteInternal(_documentsGenerator.CreateDocuments(sources), analyzer);
        }

        /// <inheritdoc />
        public Diagnostic[] ExtractDiagnostics(Document document, DiagnosticAnalyzer analyzer)
        {
            return ExecuteInternal(new[] { document }, analyzer);
        }

        /// <inheritdoc />
        public Diagnostic[] ExtractDiagnostics(Document[] documents, DiagnosticAnalyzer analyzer)
        {
            return ExecuteInternal(documents, analyzer);
        }

        /// <summary>
        /// General method that gets a collection of actual diagnostics found in the source after the analyzer is run,
        /// then verifies each of them.
        /// </summary>
        /// <param name="documents">An array of strings to create source documents from to run the analyzers on</param>
        /// <param name="analyzer">The analyzer to be run on the source code</param>
        /// <returns>DiagnosticResult for each Diagnostic</returns>
        private static Diagnostic[] ExecuteInternal(Document[] documents, DiagnosticAnalyzer analyzer)
        {
            var diagnostics = GetDiagnosticsFromDocuments(documents, analyzer);

            return SortDiagnostics(diagnostics).ToArray();
        }

        /// <summary>
        /// Sort diagnostics by location in source document
        /// </summary>
        /// <param name="diagnostics">The list of Diagnostics to be sorted</param>
        /// <returns>An IEnumerable containing the Diagnostics in order of Location</returns>
        private static IOrderedEnumerable<Diagnostic> SortDiagnostics(IEnumerable<Diagnostic> diagnostics)
        {
            return diagnostics.OrderBy(d => d.Location.SourceSpan.Start);
        }

        private static IEnumerable<Diagnostic> GetDiagnosticsFromDocuments(Document[] documents, DiagnosticAnalyzer analyzer)
        {
            return documents.GroupBy(doc => doc.Project)
                            .SelectMany(groupedDocs => GetDiagnosticsFromProject(groupedDocs.Key, groupedDocs.ToArray(), analyzer));
        }

        private static IEnumerable<Diagnostic> GetDiagnosticsFromProject(Project project,
                                                                         Document[] documents,
                                                                         DiagnosticAnalyzer analyzer)
        {
            return project.GetCompilationAsync()
                          .Result
                          .WithAnalyzers(ImmutableArray.Create(analyzer))
                          .GetAnalyzerDiagnosticsAsync()
                          .Result
                          .SelectMany(diag =>
                                      {
                                          if (diag.Location == Location.None || diag.Location.IsInMetadata)
                                          {
                                              return new[] { diag };
                                          }

                                          return documents.Select(document => document.GetSyntaxTreeAsync().Result)
                                                          .Where(tree => tree == diag.Location.SourceTree)
                                                          .Select(tree => diag);
                                      });
        }
    }
}