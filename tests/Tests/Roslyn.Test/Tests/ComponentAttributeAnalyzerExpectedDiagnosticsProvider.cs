namespace SpaceEngineers.Core.Roslyn.Test.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Abstractions;
    using Analyzers.Api;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Analyzers;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Implementations;
    using Microsoft.CodeAnalysis;
    using Sources.ComponentAttributeAnalyzer;
    using ValueObjects;

    [Component(EnLifestyle.Singleton)]
    internal class ComponentAttributeAnalyzerExpectedDiagnosticsProvider : ExpectedDiagnosticsProviderBase,
                                                                           IResolvable<IExpectedDiagnosticsProvider>,
                                                                           IResolvable<ComponentAttributeAnalyzerExpectedDiagnosticsProvider>
    {
        protected override IDictionary<string, ImmutableArray<ExpectedDiagnostic>> ExpectedInternal(
            SyntaxAnalyzerBase analyzer,
            Func<(string, int, int, DiagnosticSeverity, string), ExpectedDiagnostic> expected)
        {
            var a = (ComponentAttributeAnalyzer)analyzer;

            return new Dictionary<string, ImmutableArray<ExpectedDiagnostic>>
                   {
                       [nameof(EmptyAttributesListSource)] = ImmutableArray.Create(expected((nameof(EmptyAttributesListSource), 5, 20, DiagnosticSeverity.Error, a.MarkWithComponentAttribute))),
                       [nameof(FixWithLeadingTriviaSource)] = ImmutableArray.Create(expected((nameof(FixWithLeadingTriviaSource), 8, 20, DiagnosticSeverity.Error, a.MarkWithComponentAttribute))),
                       [nameof(FixWithoutLeadingTriviaSource)] = ImmutableArray.Create(expected((nameof(FixWithoutLeadingTriviaSource), 5, 20, DiagnosticSeverity.Error, a.MarkWithComponentAttribute))),
                       [nameof(FixWithLeadingTriviaAndAttributesSource)] = ImmutableArray.Create(expected((nameof(FixWithLeadingTriviaAndAttributesSource), 10, 20, DiagnosticSeverity.Error, a.MarkWithComponentAttribute))),
                       [nameof(NotEmptyAttributesListSource)] = ImmutableArray.Create(expected((nameof(NotEmptyAttributesListSource), 7, 20, DiagnosticSeverity.Error, a.MarkWithComponentAttribute)))
                   };
        }
    }
}