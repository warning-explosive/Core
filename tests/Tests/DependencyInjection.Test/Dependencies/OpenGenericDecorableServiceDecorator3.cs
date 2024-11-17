namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

[Component(EnLifestyle.Transient)]
internal class OpenGenericDecorableServiceDecorator3<T> : IOpenGenericDecorableService<T>
{
    public OpenGenericDecorableServiceDecorator3(IOpenGenericDecorableService<T> decorateee)
    {
        Decoratee = decorateee;
    }

    public IOpenGenericDecorableService<T> Decoratee { get; }
}