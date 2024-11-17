namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

[Component(EnLifestyle.Transient)]
internal class WithInjectedDependencyContainer : IWithInjectedDependencyContainer
{
    public WithInjectedDependencyContainer(IDependencyContainer dependencyContainer)
    {
        DependencyContainer = dependencyContainer;
    }

    public IDependencyContainer DependencyContainer { get; }
}