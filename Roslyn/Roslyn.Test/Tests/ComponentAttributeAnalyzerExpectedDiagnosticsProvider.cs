namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using AutoWiring.Api.Analyzers;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics.Roslyn;
    using Internals;
    using Microsoft.CodeAnalysis;
    using Sources.ComponentAttributeAnalyzer;
    using ValueObjects;

    [Component(EnLifestyle.Singleton)]
    internal class ComponentAttributeAnalyzerExpectedDiagnosticsProvider : ExpectedDiagnosticsProviderBase
    {
        protected override IDictionary<string, ImmutableArray<ExpectedDiagnostic>> ExpectedInternal(
            SyntaxAnalyzerBase analyzer,
            Func<(string, int, int, DiagnosticSeverity, string), ExpectedDiagnostic> expected)
        {
            var a = (ComponentAttributeAnalyzer)analyzer;

            return new Dictionary<string, ImmutableArray<ExpectedDiagnostic>>
                   {
                       [nameof(EmptyAttributesListSource)] = ImmutableArray.Create(expected((nameof(EmptyAttributesListSource), 3, 20, DiagnosticSeverity.Error, a.MarkWithComponentAttribute))),
                       [nameof(FixWithLeadingTriviaSource)] = ImmutableArray.Create(expected((nameof(FixWithLeadingTriviaSource), 6, 20, DiagnosticSeverity.Error, a.MarkWithComponentAttribute))),
                       [nameof(FixWithoutLeadingTriviaSource)] = ImmutableArray.Create(expected((nameof(FixWithoutLeadingTriviaSource), 3, 20, DiagnosticSeverity.Error, a.MarkWithComponentAttribute))),
                       [nameof(FixWithLeadingTriviaAndAttributesSource)] = ImmutableArray.Create(expected((nameof(FixWithLeadingTriviaAndAttributesSource), 9, 20, DiagnosticSeverity.Error, a.MarkWithComponentAttribute))),
                       [nameof(NotEmptyAttributesListSource)] = ImmutableArray.Create(expected((nameof(NotEmptyAttributesListSource), 7, 20, DiagnosticSeverity.Error, a.MarkWithComponentAttribute)))
                   };
        }
    }
}