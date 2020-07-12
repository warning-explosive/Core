namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Abstractions;

    internal interface IWithInjectedDependencyContainer : IResolvable
    {
        IDependencyContainer InjectedDependencyContainer { get; }
    }
}