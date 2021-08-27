namespace SpaceEngineers.Core.Roslyn.Test.Abstractions
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Analyzers.Api;
    using AutoRegistration.Api.Abstractions;
    using ValueObjects;

    /// <summary>
    /// IExpectedResultsProvider
    /// </summary>
    public interface IExpectedDiagnosticsProvider : IResolvable
    {
        /// <summary>
        /// Gets expected diagnostics
        /// </summary>
        /// <param name="analyzer">Identified diagnostic analyzer</param>
        /// <returns>Expected diagnostics</returns>
        IDictionary<string, ImmutableArray<ExpectedDiagnostic>> ByFileName(SyntaxAnalyzerBase analyzer);
    }
}