namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    internal interface IWithInjectedDependencyContainer
    {
        IDependencyContainer DependencyContainer { get; }
    }
}