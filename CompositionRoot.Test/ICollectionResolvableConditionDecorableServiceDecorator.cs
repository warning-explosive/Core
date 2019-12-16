namespace SpaceEngineers.Core.CompositionRoot.Test
{
    internal interface ICollectionResolvableConditionDecorableServiceDecorator : ICollectionResolvableConditionDecorableService
    {
        ICollectionResolvableConditionDecorableService Decoratee { get; }
    }
}