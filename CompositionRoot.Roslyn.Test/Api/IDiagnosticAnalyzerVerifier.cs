namespace SpaceEngineers.Core.CompositionRoot.Roslyn.Test.Api
{
    using Abstractions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// Verify correctness of DiagnosticAnalyzer
    /// </summary>
    public interface IDiagnosticAnalyzerVerifier : IResolvable
    {
        /// <summary>
        /// Checks each of the actual Diagnostics found and compares them with the corresponding DiagnosticResult in the array of expected results.
        /// Diagnostics are considered equal only if the DiagnosticResultLocation, Id, Severity, and Message of the DiagnosticResult match the actual diagnostic.
        /// </summary>
        /// <param name="analyzer">The analyzer that was being run on the sources</param>
        /// <param name="actualResults">Actual diagnostics</param>
        /// <param name="expectedResults">Diagnostic Results that should have appeared in the code</param>
        public void VerifyDiagnostics(DiagnosticAnalyzer analyzer,
                                      Diagnostic[] actualResults,
                                      params DiagnosticResult[] expectedResults);
    }
}