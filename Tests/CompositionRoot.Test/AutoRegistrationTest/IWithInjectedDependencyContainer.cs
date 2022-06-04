namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using Api.Abstractions;

    internal interface IWithInjectedDependencyContainer
    {
        IDependencyContainer DependencyContainer { get; }
    }
}