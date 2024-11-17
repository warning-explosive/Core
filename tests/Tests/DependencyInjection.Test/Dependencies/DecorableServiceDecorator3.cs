namespace SpaceEngineers.Core.DependencyInjection.Test.Dependencies;

[Component(EnLifestyle.Transient)]
internal class DecorableServiceDecorator3 : IDecorableService
{
    public DecorableServiceDecorator3(IDecorableService decoratorType)
    {
        Decoratee = decoratorType;
    }

    public IDecorableService Decoratee { get; }
}