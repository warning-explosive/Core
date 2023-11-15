namespace SpaceEngineers.Core.Roslyn.Test.Sources.ComponentAttributeAnalyzer
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    [Serializable]
    internal class ExistedAttributeSource : ITestService, IResolvable<ITestService>
    {
    }
}