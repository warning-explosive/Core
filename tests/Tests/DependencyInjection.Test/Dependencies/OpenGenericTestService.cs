namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

[Component(EnLifestyle.Transient)]
internal class OpenGenericTestService<T> : IOpenGenericTestService<T>
{
}