namespace SpaceEngineers.Core.Roslyn.Test.Sources.ComponentAttributeAnalyzerExpected
{
    using AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    /*<Analyzer Name="ComponentAttributeAnalyzer">[Component(EnLifestyle.ChooseLifestyle)]</Analyzer>*/
    internal class FixWithoutLeadingTriviaSourceExpected : ITestService, IResolvable<ITestService>
    {
    }
}