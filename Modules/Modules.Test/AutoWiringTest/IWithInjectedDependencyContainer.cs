namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Abstractions;

    internal interface IWithInjectedDependencyContainer : IResolvable
    {
        IDependencyContainer DependencyContainer { get; }

        IVersionedContainer VersionedContainer { get; }

        IRegistrationContainer RegistrationContainer { get; }

        IScopedContainer ScopedContainer { get; }
    }
}