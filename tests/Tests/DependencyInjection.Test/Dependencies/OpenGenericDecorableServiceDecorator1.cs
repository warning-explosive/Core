namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

[Component(EnLifestyle.Transient)]
[After(typeof(OpenGenericDecorableServiceDecorator2<>))]
internal class OpenGenericDecorableServiceDecorator1<T> : IOpenGenericDecorableService<T>
{
    public OpenGenericDecorableServiceDecorator1(IOpenGenericDecorableService<T> decorateee)
    {
        Decoratee = decorateee;
    }

    public IOpenGenericDecorableService<T> Decoratee { get; }
}