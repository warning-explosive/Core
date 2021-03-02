namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Abstractions;

    internal interface IWithInjectedDependencyContainer : IResolvable
    {
        IDependencyContainer DependencyContainer { get; }

        IVersionedContainer VersionedContainer { get; }

        IRegistrationContainer RegistrationContainer { get; }

        IScopedContainer ScopedContainer { get; }
    }
}