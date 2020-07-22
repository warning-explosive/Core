namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Abstractions;

    internal interface IServiceWithDecoratedDependency : IResolvable
    {
        IServiceForInterception ServiceForInterception { get; }
    }
}