namespace SpaceEngineers.Core.Roslyn.Test.Sources.LifestyleAttributeAnalyzer
{
    using System;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Singleton)]
    [Serializable]
    internal class ExistedAttributeSource : ITestService
    {
    }
}