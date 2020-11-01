namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics.Roslyn;
    using Internals;
    using Microsoft.CodeAnalysis;
    using Sources.LifestyleAttributeAnalyzer;
    using ValueObjects;

    [Lifestyle(EnLifestyle.Singleton)]
    internal class LifestyleAttributeAnalyzerExpectedDiagnosticsProvider : ExpectedDiagnosticsProviderBase
    {
        protected override IDictionary<string, ImmutableArray<ExpectedDiagnostic>> ExpectedInternal(
            SyntaxAnalyzerBase analyzer,
            Func<(string, int, int, DiagnosticSeverity, string), ExpectedDiagnostic> expected)
        {
            return new Dictionary<string, ImmutableArray<ExpectedDiagnostic>>
                   {
                       [nameof(EmptyAttributesListSource)] = ImmutableArray.Create(expected((nameof(EmptyAttributesListSource), 3, 20, DiagnosticSeverity.Error, analyzer.Message))),
                       [nameof(FixWithLeadingTriviaSource)] = ImmutableArray.Create(expected((nameof(FixWithLeadingTriviaSource), 6, 20, DiagnosticSeverity.Error, analyzer.Message))),
                       [nameof(FixWithoutLeadingTriviaSource)] = ImmutableArray.Create(expected((nameof(FixWithoutLeadingTriviaSource), 3, 20, DiagnosticSeverity.Error, analyzer.Message))),
                       [nameof(FixWithLeadingTriviaAndAttributesSource)] = ImmutableArray.Create(expected((nameof(FixWithLeadingTriviaAndAttributesSource), 9, 20, DiagnosticSeverity.Error, analyzer.Message))),
                       [nameof(NotEmptyAttributesListSource)] = ImmutableArray.Create(expected((nameof(NotEmptyAttributesListSource), 7, 20, DiagnosticSeverity.Error, analyzer.Message)))
                   };
        }
    }
}