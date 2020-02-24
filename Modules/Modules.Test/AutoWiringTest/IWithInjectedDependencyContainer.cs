namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoRegistration;
    using AutoWiringApi.Abstractions;

    internal interface IWithInjectedDependencyContainer : IResolvable
    {
        DependencyContainer InjectedDependencyContainer { get; }
    }
}