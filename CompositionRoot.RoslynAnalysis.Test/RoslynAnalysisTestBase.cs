namespace SpaceEngineers.Core.CompositionRoot.RoslynAnalysis.Test
{
    using System;
    using Api;
    using CompositionRoot.Test;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Base class for Roslyn analysis testing
    /// </summary>
    public abstract class RoslynAnalysisTestBase : CompositionRootTestBase
    {
        private readonly IDiagnosticAnalyzerVerifier _analyzerVerifier;
        private readonly ICodeFixVerifier _codeFixVerifier;

        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        protected RoslynAnalysisTestBase(ITestOutputHelper output)
            : base(output)
        {
            _analyzerVerifier = DependencyContainer.Resolve<IDiagnosticAnalyzerVerifier>();
            _codeFixVerifier = DependencyContainer.Resolve<ICodeFixVerifier>();
        }

        /// <summary>
        /// DiagnosticAnalyzer
        /// </summary>
        protected abstract DiagnosticAnalyzer DiagnosticAnalyzer { get; }

        /// <summary>
        /// CodeFixProvider
        /// </summary>
        protected abstract CodeFixProvider CodeFixProvider { get; }

        /// <summary>
        /// Default test with empty source
        /// </summary>
        [Fact]
        public void EmptyTest()
        {
            VerifyAnalyzer(string.Empty, Array.Empty<DiagnosticResult>());
        }

        /// <summary>
        /// Verify DiagnosticAnalyzer
        /// </summary>
        /// <param name="source">A class in the form of a string</param>
        /// <param name="expected">Diagnostic Results that should have appeared in the code</param>
        protected void VerifyAnalyzer(string source, params DiagnosticResult[] expected)
        {
            _analyzerVerifier.VerifyDiagnostics(source, DiagnosticAnalyzer, expected);
        }

        /// <summary>
        /// Verify CodeFixProvider related to DiagnosticAnalyzer
        /// </summary>
        /// <param name="source">A class in the form of a string</param>
        /// <param name="newSource">Expected class in the form of a string</param>
        protected void VerifyFix(string source, string newSource)
        {
            _codeFixVerifier.VerifyFix(DiagnosticAnalyzer, CodeFixProvider, source, newSource);
        }
    }
}
