namespace SpaceEngineers.Core.CompositionRoot.RoslynAnalysis.Test
{
    using System;
    using System.Linq;
    using Api;
    using Basics;
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
        private readonly IDiagnosticsAnalyzerExecutor _analyzerExecutor;
        private readonly IDiagnosticAnalyzerVerifier _analyzerVerifier;
        private readonly ICodeFixExecutor _codeFixExecutor;
        private readonly ICodeFixVerifyer _codeFixVerifyer;

        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        protected RoslynAnalysisTestBase(ITestOutputHelper output)
            : base(output)
        {
            _analyzerExecutor = DependencyContainer.Resolve<IDiagnosticsAnalyzerExecutor>();
            _analyzerVerifier = DependencyContainer.Resolve<IDiagnosticAnalyzerVerifier>();
            _codeFixExecutor = DependencyContainer.Resolve<ICodeFixExecutor>();
            _codeFixVerifyer = DependencyContainer.Resolve<ICodeFixVerifyer>();
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
            var actualDiagnostics = _analyzerExecutor.ExtractDiagnostics(source, DiagnosticAnalyzer);

            if (actualDiagnostics.Any())
            {
                actualDiagnostics.Each((z, i) => Output.WriteLine($"Diagnostic {i + 1}:\n\t" + source.Substring(z.Location.SourceSpan.Start, z.Location.SourceSpan.Length)));
            }
            else
            {
                Output.WriteLine("Actual diagnostics is empty");
            }

            _analyzerVerifier.VerifyDiagnostics(DiagnosticAnalyzer, actualDiagnostics, expected);
        }

        /// <summary>
        /// Verify CodeFixProvider related to DiagnosticAnalyzer
        /// </summary>
        /// <param name="inputSource">A class in the form of a string</param>
        /// <param name="expectedSource">Expected class in the form of a string</param>
        /// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
        protected void VerifyFix(string inputSource,
                                 string expectedSource,
                                 bool allowNewCompilerDiagnostics)
        {
            var actualSource = _codeFixExecutor.ExecuteFix(DiagnosticAnalyzer, CodeFixProvider, inputSource, allowNewCompilerDiagnostics);

            _codeFixVerifyer.VerifyCodeFix(expectedSource, actualSource);
        }
    }
}
