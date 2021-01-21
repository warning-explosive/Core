namespace SpaceEngineers.Core.Roslyn.Test.Sources.LifestyleAttributeAnalyzer
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [LifestyleAttribute(EnLifestyle.Transient)]
    [Unregistered]
    internal class RemoveAttributeSource : ITestService
    {
    }
}