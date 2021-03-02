namespace SpaceEngineers.Core.Roslyn.Test.Abstractions
{
    using System.Collections.Immutable;
    using AutoWiring.Api.Abstractions;
    using Basics.Roslyn;
    using ValueObjects;

    /// <summary>
    /// Verify correctness of DiagnosticAnalyzer
    /// </summary>
    public interface IDiagnosticAnalyzerVerifier : IResolvable
    {
        /// <summary>
        /// VerifyDiagnostics by IExpectedResultsProvider
        /// </summary>
        /// <param name="analyzer">The analyzer that was being run on the sources</param>
        /// <param name="analyzedDocument">AnalyzedDocument</param>
        /// <param name="expectedDiagnostics">Expected diagnostics</param>
        public void VerifyAnalyzedDocument(SyntaxAnalyzerBase analyzer,
                                           AnalyzedDocument analyzedDocument,
                                           ImmutableArray<ExpectedDiagnostic> expectedDiagnostics);
    }
}