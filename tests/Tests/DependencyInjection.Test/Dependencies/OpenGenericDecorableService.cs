namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

[Component(EnLifestyle.Transient)]
internal class OpenGenericDecorableService<T> : IOpenGenericDecorableService<T>
{
}