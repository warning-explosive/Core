namespace SpaceEngineers.Core.Roslyn.Test.Abstractions
{
    using System;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using ValueObjects;

    /// <summary>
    /// Verifies code fix results
    /// </summary>
    public interface ICodeFixVerifier : IResolvable
    {
        /// <summary>
        /// Verify code fix results
        /// </summary>
        /// <param name="analyzer">DiagnosticAnalyzer</param>
        /// <param name="codeFix">CodeFixProvider</param>
        /// <param name="analyzedDocument">AnalyzedDocument</param>
        /// <param name="expectedSource">CodeFix expected source</param>
        /// <param name="show">Show result</param>
        /// <returns>Ongoing operation</returns>
        Task VerifyCodeFix(DiagnosticAnalyzer analyzer,
                           CodeFixProvider codeFix,
                           AnalyzedDocument analyzedDocument,
                           SourceFile expectedSource,
                           Action<string> show);
    }
}