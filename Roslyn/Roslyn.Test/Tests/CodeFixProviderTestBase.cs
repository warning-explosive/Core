namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System;
    using Api;
    using Basics.Roslyn;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Xunit.Abstractions;

    /// <summary>
    /// Base class for CodeFixProvider testing
    /// </summary>
    /// <typeparam name="TAnalyzer">DiagnosticAnalyzer type-argument</typeparam>
    /// <typeparam name="TCodeFix">CodeFixProvider type-argument</typeparam>
    public abstract class CodeFixProviderTestBase<TAnalyzer, TCodeFix> : DiagnosticAnalyzerTestBase<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, IIdentifiedAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        private readonly ICodeFixExecutor _codeFixExecutor;
        private readonly ICodeFixVerifier _codeFixVerifier;

        private readonly TAnalyzer _diagnosticAnalyzer;
        private readonly TCodeFix _codeFixProvider;

        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        protected CodeFixProviderTestBase(ITestOutputHelper output)
            : base(output)
        {
            _codeFixExecutor = DependencyContainer.Resolve<ICodeFixExecutor>();
            _codeFixVerifier = DependencyContainer.Resolve<ICodeFixVerifier>();

            _diagnosticAnalyzer = new TAnalyzer();
            _codeFixProvider = new TCodeFix();
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
            var actualSource = _codeFixExecutor.ExecuteFix(_diagnosticAnalyzer, _codeFixProvider, inputSource, allowNewCompilerDiagnostics);

            Output.WriteLine(Environment.NewLine + nameof(actualSource) + ":");
            Output.WriteLine(actualSource);

            _codeFixVerifier.VerifyCodeFix(expectedSource, actualSource);
        }
    }
}
