namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(1)]
    public class CollectionResolvableConditionDecorableServiceDecorator2 : ICollectionResolvableConditionDecorableServiceDecorator,
                                                                           ICollectionConditionalDecorator<ICollectionResolvableConditionDecorableService, TestConditionAttribute2>
    {
        public ICollectionResolvableConditionDecorableService Decoratee { get; }

        public CollectionResolvableConditionDecorableServiceDecorator2(ICollectionResolvableConditionDecorableService decoratee)
        {
            Decoratee = decoratee;
        }
    }
}