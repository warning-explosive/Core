namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

[Component(EnLifestyle.Transient)]
internal class ClosedGenericImplementationOfOpenGenericService : IOpenGenericTestService<ExternalResolvable>
{
}