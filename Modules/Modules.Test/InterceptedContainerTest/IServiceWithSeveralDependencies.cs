namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Abstractions;

    internal interface IServiceWithSeveralDependencies : IResolvable
    {
        IServiceForInterception ServiceForInterception { get; }

        IServiceWithDecoratedDependency ServiceWithDecoratedDependency { get; }
    }
}