namespace SpaceEngineers.Core.Roslyn.Test.Sources.LifestyleAttributeAnalyzer
{
    using System;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    [Serializable]
    internal class ExistedAttributeSource : ITestService
    {
    }
}