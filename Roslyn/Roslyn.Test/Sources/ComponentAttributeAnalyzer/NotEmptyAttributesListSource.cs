namespace SpaceEngineers.Core.Roslyn.Test.Sources.ComponentAttributeAnalyzer
{
    using System;
    using AutoWiring.Api.Abstractions;

    [Serializable]
    internal class NotEmptyAttributesListSource : ICollectionResolvable<ITestCollectionResolvableService>
    {
    }
}