namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Enumerations;
    using Extensions.Attributes;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(1)]
    public class CollectionResolvableConditionDecorableServiceDecorator1 : ICollectionResolvableConditionDecorableServiceDecorator,
                                                                           ICollectionConditionalDecorator<ICollectionResolvableConditionDecorableService, TestConditionAttribute1>
    {
        public ICollectionResolvableConditionDecorableService Decoratee { get; }

        public CollectionResolvableConditionDecorableServiceDecorator1(ICollectionResolvableConditionDecorableService decoratee)
        {
            Decoratee = decoratee;
        }
    }
}