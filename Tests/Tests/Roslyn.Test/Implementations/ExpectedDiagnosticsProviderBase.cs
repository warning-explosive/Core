namespace SpaceEngineers.Core.Roslyn.Test.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Abstractions;
    using Analyzers.Api;
    using Microsoft.CodeAnalysis;
    using ValueObjects;

    internal abstract class ExpectedDiagnosticsProviderBase : IExpectedDiagnosticsProvider
    {
        public IDictionary<string, ImmutableArray<ExpectedDiagnostic>> ByFileName(SyntaxAnalyzerBase analyzer)
        {
            return ExpectedInternal(analyzer, Compose(analyzer));
        }

        protected abstract IDictionary<string, ImmutableArray<ExpectedDiagnostic>> ExpectedInternal(
            SyntaxAnalyzerBase analyzer,
            Func<(string, int, int, DiagnosticSeverity, string), ExpectedDiagnostic> expected);

        private static Func<(string, int, int, DiagnosticSeverity, string), ExpectedDiagnostic> Compose(SyntaxAnalyzerBase analyzer)
        {
            return args =>
                   {
                       var (sourceFileName, line, column, severity, msg) = args;
                       return new ExpectedDiagnostic(analyzer.SupportedDiagnostics.Single(), msg, severity)
                          .WithLocation(sourceFileName, line, column);
                   };
        }
    }
}