namespace SpaceEngineers.Core.Roslyn.Test.Sources.ComponentAttributeAnalyzerExpected
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    /// <summary>
    /// Summary
    /// </summary>
    [Serializable]
    /*<Analyzer Name="ComponentAttributeAnalyzer">[Component(EnLifestyle.ChooseLifestyle)]</Analyzer>*/
    internal class FixWithLeadingTriviaAndAttributesSourceExpected : ITestService, IResolvable<ITestService>
    {
    }
}