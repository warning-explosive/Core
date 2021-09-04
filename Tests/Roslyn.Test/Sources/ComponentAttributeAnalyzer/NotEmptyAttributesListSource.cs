namespace SpaceEngineers.Core.Roslyn.Test.Sources.ComponentAttributeAnalyzer
{
    using System;
    using AutoRegistration.Api.Abstractions;

    [Serializable]
    internal class NotEmptyAttributesListSource : ICollectionResolvable<ITestCollectionResolvableService>
    {
    }
}