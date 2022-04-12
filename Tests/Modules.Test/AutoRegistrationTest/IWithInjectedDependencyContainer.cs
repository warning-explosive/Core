namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using CompositionRoot.Api.Abstractions;

    internal interface IWithInjectedDependencyContainer
    {
        IDependencyContainer DependencyContainer { get; }
    }
}