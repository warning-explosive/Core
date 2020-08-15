namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [ManualRegistration]
    internal class ConcreteRegisteredByDelegate : IResolvable
    {
    }
}