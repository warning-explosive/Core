namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Abstractions;

    internal interface IWithInjectedDependencyContainer : IResolvable
    {
        IDependencyContainer DependencyContainer { get; }

        IScopedContainer ScopedContainer { get; }
    }
}