namespace SpaceEngineers.Core.Roslyn.Test.Sources.LifestyleAttributeAnalyzer
{
    using System;
    using AutoWiringApi.Abstractions;

    [Serializable]
    internal class NotEmptyAttributesListSource : ICollectionResolvable<ITestCollectionResolvableService>
    {
    }
}