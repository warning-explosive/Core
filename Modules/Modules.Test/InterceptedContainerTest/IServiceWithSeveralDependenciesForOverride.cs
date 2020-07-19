namespace SpaceEngineers.Core.Modules.Test.InterceptedContainerTest
{
    using AutoWiringApi.Abstractions;

    internal interface IServiceWithSeveralDependenciesForOverride : IResolvable
    {
        IServiceForInterception ServiceForInterception { get; }

        IServiceWithOverrideAsDependency ServiceWithOverrideAsDependency { get; }
    }
}