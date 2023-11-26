namespace SpaceEngineers.Core.Roslyn.Test.Abstractions
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using ValueObjects;

    /// <summary>
    /// Analysis objects provider base on conventions
    /// </summary>
    public interface IConventionalProvider
    {
        /// <summary>
        /// GetCodeFixProvider
        /// </summary>
        /// <param name="analyzer">DiagnosticAnalyzer</param>
        /// <returns>CodeFixProvider</returns>
        CodeFixProvider? CodeFixProvider(DiagnosticAnalyzer analyzer);

        /// <summary>
        /// GetExpectedDiagnosticsProvider
        /// </summary>
        /// <param name="analyzer">DiagnosticAnalyzer</param>
        /// <returns>IExpectedDiagnosticsProvider</returns>
        IExpectedDiagnosticsProvider ExpectedDiagnosticsProvider(DiagnosticAnalyzer analyzer);

        /// <summary>
        /// GetSourceFile
        /// </summary>
        /// <param name="analyzer">DiagnosticAnalyzer</param>
        /// <param name="directorySuffix">DirectorySuffix</param>
        /// <returns>SourceFiles</returns>
        public IEnumerable<SourceFile> SourceFiles(DiagnosticAnalyzer analyzer, string? directorySuffix = null);
    }
}