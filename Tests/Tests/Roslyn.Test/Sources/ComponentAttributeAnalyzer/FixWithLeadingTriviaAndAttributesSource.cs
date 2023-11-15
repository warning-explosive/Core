namespace SpaceEngineers.Core.Roslyn.Test.Sources.ComponentAttributeAnalyzer
{
    using System;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// Summary
    /// </summary>
    [Serializable]
    internal class FixWithLeadingTriviaAndAttributesSource : ITestService, IResolvable<ITestService>
    {
    }
}