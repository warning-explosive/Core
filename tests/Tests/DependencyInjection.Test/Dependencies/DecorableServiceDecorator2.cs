namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

[Component(EnLifestyle.Transient)]
[After(typeof(DecorableServiceDecorator3))]
internal class DecorableServiceDecorator2 : IDecorableService
{
    public DecorableServiceDecorator2(IDecorableService decoratorType)
    {
        Decoratee = decoratorType;
    }

    public IDecorableService Decoratee { get; }
}