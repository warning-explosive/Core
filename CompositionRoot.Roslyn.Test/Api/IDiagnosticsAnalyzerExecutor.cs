namespace SpaceEngineers.Core.CompositionRoot.Roslyn.Test.Api
{
    using Abstractions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// Executes C# DiagnosticAnalyzer on source documents or strings and receives Diagnostics array
    /// </summary>
    public interface IDiagnosticsAnalyzerExecutor : IResolvable
    {
        /// <summary>
        /// Called to receive a Diagnostics array from C# DiagnosticAnalyzer when applied on the single inputted string as a source
        /// </summary>
        /// <param name="source">A class in the form of a string</param>
        /// <param name="analyzer">CSharp analyzer being tested - to be implemented in non-abstract class</param>
        /// <returns>DiagnosticResult for each Diagnostic</returns>
        Diagnostic[] ExtractDiagnostics(string source, DiagnosticAnalyzer analyzer);

        /// <summary>
        /// Called to receive a Diagnostics array from C# DiagnosticAnalyzer when applied on the inputted strings as a source
        /// </summary>
        /// <param name="sources">An array of source strings</param>
        /// <param name="analyzer">CSharp analyzer being tested - to be implemented in non-abstract class</param>
        /// <returns>DiagnosticResult for each Diagnostic</returns>
        Diagnostic[] ExtractDiagnostics(string[] sources, DiagnosticAnalyzer analyzer);

        /// <summary>
        /// Called to receive a Diagnostics array from C# DiagnosticAnalyzer when applied on the single inputted source document
        /// </summary>
        /// <param name="document">Source document</param>
        /// <param name="analyzer">CSharp analyzer being tested - to be implemented in non-abstract class</param>
        /// <returns>DiagnosticResult for each Diagnostic</returns>
        Diagnostic[] ExtractDiagnostics(Document document, DiagnosticAnalyzer analyzer);

        /// <summary>
        /// Called to receive a Diagnostics array from C# DiagnosticAnalyzer when applied on the inputted source documents
        /// </summary>
        /// <param name="documents">An array of source documents</param>
        /// <param name="analyzer">CSharp analyzer being tested - to be implemented in non-abstract class</param>
        /// <returns>DiagnosticResult for each Diagnostic</returns>
        Diagnostic[] ExtractDiagnostics(Document[] documents, DiagnosticAnalyzer analyzer);
    }
}