namespace SpaceEngineers.Core.Roslyn.Test.Sources.LifestyleAttributeAnalyzer
{
    using System;
    using AutoWiring.Api.Abstractions;

    [Serializable]
    internal class NotEmptyAttributesListSource : ICollectionResolvable<ITestCollectionResolvableService>
    {
    }
}