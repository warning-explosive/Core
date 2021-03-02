namespace SpaceEngineers.Core.Roslyn.Test.Sources.LifestyleAttributeAnalyzer
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [LifestyleAttribute(EnLifestyle.Transient)]
    [Unregistered]
    internal class RemoveAttributeSource : ITestService
    {
    }
}