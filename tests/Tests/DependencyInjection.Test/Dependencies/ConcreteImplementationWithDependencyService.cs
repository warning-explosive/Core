namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

[Component(EnLifestyle.Transient)]
internal class ConcreteImplementationWithDependencyService
{
    public ConcreteImplementationWithDependencyService(ConcreteImplementationService dependency)
    {
        Dependency = dependency;
    }

    public ConcreteImplementationService Dependency { get; }
}