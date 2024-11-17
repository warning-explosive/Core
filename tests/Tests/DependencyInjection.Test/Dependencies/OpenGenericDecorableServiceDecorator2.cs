namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

[Component(EnLifestyle.Transient)]
[After(typeof(OpenGenericDecorableServiceDecorator3<>))]
internal class OpenGenericDecorableServiceDecorator2<T> : IOpenGenericDecorableService<T>
{
    public OpenGenericDecorableServiceDecorator2(IOpenGenericDecorableService<T> decorateee)
    {
        Decoratee = decorateee;
    }

    public IOpenGenericDecorableService<T> Decoratee { get; }
}