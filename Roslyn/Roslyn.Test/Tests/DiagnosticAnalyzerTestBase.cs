namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System;
    using System.Linq;
    using Api;
    using AutoRegistration;
    using Basics;
    using Basics.Roslyn;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Base class for DiagnosticAnalyzer tests
    /// </summary>
    /// <typeparam name="TAnalyzer">DiagnosticAnalyzer type-argument</typeparam>
    public abstract class DiagnosticAnalyzerTestBase<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, IIdentifiedAnalyzer, new()
    {
        private readonly IDiagnosticsAnalyzerExecutor _analyzerExecutor;
        private readonly IDiagnosticAnalyzerVerifier _analyzerVerifier;

        private readonly TAnalyzer _diagnosticAnalyzer;

        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        protected DiagnosticAnalyzerTestBase(ITestOutputHelper output)
        {
            Output = output;

            DependencyContainer = AutoRegistration.DependencyContainer.Default(typeof(DiagnosticAnalyzerTestBase<>).Assembly);

            _analyzerExecutor = DependencyContainer.Resolve<IDiagnosticsAnalyzerExecutor>();
            _analyzerVerifier = DependencyContainer.Resolve<IDiagnosticAnalyzerVerifier>();

            _diagnosticAnalyzer = new TAnalyzer();
        }

        /// <summary>
        /// ITestOutputHelper
        /// </summary>
        protected ITestOutputHelper Output { get; }

        /// <summary>
        /// DependencyContainer
        /// </summary>
        protected IDependencyContainer DependencyContainer { get; }

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
            var actualDiagnostics = _analyzerExecutor.ExtractDiagnostics(source, _diagnosticAnalyzer);

            if (actualDiagnostics.Any())
            {
                actualDiagnostics.Each((z, i) => Output.WriteLine($"Diagnostic {i + 1}:\n\t" + source.Substring(z.Location.SourceSpan.Start, z.Location.SourceSpan.Length)));
            }
            else
            {
                Output.WriteLine("Actual diagnostics is empty");
            }

            _analyzerVerifier.VerifyDiagnostics(_diagnosticAnalyzer, actualDiagnostics, expected);
        }

        /// <summary>
        /// Create expected result
        /// </summary>
        /// <param name="line">Line</param>
        /// <param name="column">Column</param>
        /// <returns>DiagnosticResult</returns>
        protected DiagnosticResult Expected(int line, int column)
        {
            return new DiagnosticResult(_diagnosticAnalyzer.Identifier,
                                        _diagnosticAnalyzer.Message,
                                        DiagnosticSeverity.Error,
                                        new[] { new DiagnosticResultLocation("Source0.cs", line, column) });
        }
    }
}