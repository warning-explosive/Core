namespace SpaceEngineers.Core.Roslyn.Test.Abstractions
{
    using AutoWiringApi.Abstractions;
    using Basics.Roslyn;
    using Microsoft.CodeAnalysis;
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
        /// <param name="actualDiagnostics">Actual diagnostics</param>
        public void VerifyDiagnosticsGroup(SyntaxAnalyzerBase analyzer, Diagnostic[] actualDiagnostics);

        /// <summary>
        /// Checks each of the actual Diagnostics found and compares them with the corresponding ExpectedDiagnostic in the array of expected results.
        /// Diagnostics are considered equal only if the DiagnosticLocation, Id, Severity, and Message of the ExpectedDiagnostic match the actual diagnostic.
        /// </summary>
        /// <param name="analyzer">The analyzer that was being run on the sources</param>
        /// <param name="actualDiagnostics">Actual diagnostics</param>
        /// <param name="expectedResults">Expected diagnostics that should have appeared in the code</param>
        public void VerifyDiagnostics(SyntaxAnalyzerBase analyzer,
                                      Diagnostic[] actualDiagnostics,
                                      params ExpectedDiagnostic[] expectedResults);
    }
}