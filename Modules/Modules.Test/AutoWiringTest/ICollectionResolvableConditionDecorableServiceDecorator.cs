namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    internal interface ICollectionResolvableConditionDecorableServiceDecorator : ICollectionResolvableConditionDecorableService
    {
        ICollectionResolvableConditionDecorableService Decoratee { get; }
    }
}