namespace SpaceEngineers.Core.CompositionRoot.Roslyn.Test.Api
{
    using Abstractions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// Verify correctness of codefixes
    /// </summary>
    public interface ICodeFixExecutor : IResolvable
    {
        /// <summary>
        /// General verifier for codefixes.
        /// Creates a Document from the source string, then gets diagnostics on it and applies the relevant codefixes.
        /// Then gets the string after the codefix is applied and compares it with the expected result.
        /// Note: If any codefix causes new diagnostics to show up, the test fails unless allowNewCompilerDiagnostics is set to true.
        /// </summary>
        /// <param name="analyzer">The analyzer to be applied to the source code</param>
        /// <param name="codeFix">The codefix to be applied to the code wherever the relevant Diagnostic is found</param>
        /// <param name="inputSource">A class in the form of a string before the CodeFix was applied to it</param>
        /// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
        /// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
        /// <returns>Fixed source code</returns>
        string ExecuteFix(DiagnosticAnalyzer analyzer,
                          CodeFixProvider codeFix,
                          string inputSource,
                          bool allowNewCompilerDiagnostics,
                          int? codeFixIndex = null);
    }
}