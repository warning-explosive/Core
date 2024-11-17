namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

[Component(EnLifestyle.Transient)]
[After(typeof(DecorableServiceDecorator2))]
internal class DecorableServiceDecorator1 : IDecorableService
{
    public DecorableServiceDecorator1(IDecorableService decoratorType)
    {
        Decoratee = decoratorType;
    }

    public IDecorableService Decoratee { get; }
}