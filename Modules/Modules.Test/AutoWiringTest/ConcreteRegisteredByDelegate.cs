namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using System.Diagnostics.CodeAnalysis;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;

    [ManualRegistration]
    [SuppressMessage("Analysis", "CR1", Justification = "Manually registered by delegate for test reasons")]
    internal class ConcreteRegisteredByDelegate : IResolvable
    {
    }
}