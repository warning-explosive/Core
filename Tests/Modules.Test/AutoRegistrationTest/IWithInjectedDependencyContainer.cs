namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Abstractions;
    using CompositionRoot.Api.Abstractions;

    internal interface IWithInjectedDependencyContainer : IResolvable
    {
        IDependencyContainer DependencyContainer { get; }

        IScopedContainer ScopedContainer { get; }
    }
}