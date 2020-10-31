namespace SpaceEngineers.Core.Roslyn.Test.Sources
{
    using System;
    using System.Collections.Generic;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics.Roslyn;
    using Internals;
    using LifestyleAttributeAnalyzer;
    using Microsoft.CodeAnalysis;
    using ValueObjects;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class LifestyleAttributeAnalyzerExpectedDiagnosticsProvider : ExpectedDiagnosticsProviderBase
    {
        protected override IDictionary<string, ExpectedDiagnostic[]> ExpectedInternal(
            SyntaxAnalyzerBase analyzer,
            Func<(string, int, int, DiagnosticSeverity, string), ExpectedDiagnostic> expected)
        {
            return new Dictionary<string, ExpectedDiagnostic[]>
                   {
                       [nameof(EmptyAttributesListSource)] = new[] { expected((nameof(EmptyAttributesListSource), 3, 20, DiagnosticSeverity.Error, analyzer.Message)) },
                       [nameof(FixWithLeadingTriviaSource)] = new[] { expected((nameof(FixWithLeadingTriviaSource), 6, 20, DiagnosticSeverity.Error, analyzer.Message)) },
                       [nameof(FixWithoutLeadingTriviaSource)] = new[] { expected((nameof(FixWithoutLeadingTriviaSource), 3, 20, DiagnosticSeverity.Error, analyzer.Message)) },
                       [nameof(FixWithLeadingTriviaAndAttributesSource)] = new[] { expected((nameof(FixWithLeadingTriviaAndAttributesSource), 9, 20, DiagnosticSeverity.Error, analyzer.Message)) },
                       [nameof(NotEmptyAttributesListSource)] = new[] { expected((nameof(NotEmptyAttributesListSource), 7, 20, DiagnosticSeverity.Error, analyzer.Message)) }
                   };
        }
    }
}