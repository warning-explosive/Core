namespace SpaceEngineers.Core.Roslyn.Test.Sources.ComponentAttributeAnalyzerExpected
{
    using AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    /// <summary>
    /// Summary
    /// </summary>
    /*<Analyzer Name="ComponentAttributeAnalyzer">[Component(EnLifestyle.ChooseLifestyle)]</Analyzer>*/
    internal class FixWithLeadingTriviaSourceExpected : ITestService, IResolvable<ITestService>
    {
    }
}