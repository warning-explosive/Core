namespace SpaceEngineers.Core.CompositionRoot.RoslynAnalysis.Test.Api
{
    using Abstractions;
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
        /// <param name="source">A class in the form of a string</param>
        /// <param name="analyzer">The analyzer that was being run on the sources</param>
        /// <param name="expectedResults">Diagnostic Results that should have appeared in the code</param>
        void VerifyDiagnostics(string source,
                               DiagnosticAnalyzer analyzer,
                               params DiagnosticResult[] expectedResults);
    }
}