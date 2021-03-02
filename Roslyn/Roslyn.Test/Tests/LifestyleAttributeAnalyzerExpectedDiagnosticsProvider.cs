namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using AutoWiring.Api.Analyzers;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
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
            var a = (LifestyleAttributeAnalyzer)analyzer;

            return new Dictionary<string, ImmutableArray<ExpectedDiagnostic>>
                   {
                       [nameof(EmptyAttributesListSource)] = ImmutableArray.Create(expected((nameof(EmptyAttributesListSource), 3, 20, DiagnosticSeverity.Error, a.MarkWithLifestyleAttribute))),
                       [nameof(FixWithLeadingTriviaSource)] = ImmutableArray.Create(expected((nameof(FixWithLeadingTriviaSource), 6, 20, DiagnosticSeverity.Error, a.MarkWithLifestyleAttribute))),
                       [nameof(FixWithoutLeadingTriviaSource)] = ImmutableArray.Create(expected((nameof(FixWithoutLeadingTriviaSource), 3, 20, DiagnosticSeverity.Error, a.MarkWithLifestyleAttribute))),
                       [nameof(FixWithLeadingTriviaAndAttributesSource)] = ImmutableArray.Create(expected((nameof(FixWithLeadingTriviaAndAttributesSource), 9, 20, DiagnosticSeverity.Error, a.MarkWithLifestyleAttribute))),
                       [nameof(NotEmptyAttributesListSource)] = ImmutableArray.Create(expected((nameof(NotEmptyAttributesListSource), 7, 20, DiagnosticSeverity.Error, a.MarkWithLifestyleAttribute))),
                       [nameof(RemoveAttributeSource)] = ImmutableArray.Create(expected((nameof(RemoveAttributeSource), 8, 20, DiagnosticSeverity.Error, string.Format(CultureInfo.InvariantCulture, a.RemoveLifestyleAttribute, nameof(UnregisteredAttribute)))))
                   };
        }
    }
}