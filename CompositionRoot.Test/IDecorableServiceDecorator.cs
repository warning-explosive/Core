namespace SpaceEngineers.Core.CompositionRoot.Test
{
    internal interface IDecorableServiceDecorator : IDecorableService
    {
        IDecorableService Decoratee { get; }
    }
}