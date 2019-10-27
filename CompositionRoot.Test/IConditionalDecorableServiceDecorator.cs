namespace SpaceEngineers.Core.CompositionRoot.Test
{
    internal interface IConditionalDecorableServiceDecorator : IConditionalDecorableService
    {
        IConditionalDecorableService Decoratee { get; }
    }
}