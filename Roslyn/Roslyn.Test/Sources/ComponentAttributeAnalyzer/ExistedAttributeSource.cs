namespace SpaceEngineers.Core.Roslyn.Test.Sources.ComponentAttributeAnalyzer
{
    using System;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    [Serializable]
    internal class ExistedAttributeSource : ITestService
    {
    }
}