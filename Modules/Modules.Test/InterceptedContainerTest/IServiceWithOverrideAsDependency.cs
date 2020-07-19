namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Abstractions;

    internal interface IServiceWithOverrideAsDependency : IResolvable
    {
        IServiceForInterception ServiceForInterception { get; }
    }
}