namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Basics.Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(1)]
    public class CollectionResolvableConditionDecorableServiceDecorator1 : ICollectionResolvableConditionDecorableServiceDecorator,
                                                                           ICollectionConditionalDecorator<ICollectionResolvableConditionDecorableService, TestCondition1Attribute>
    {
        public CollectionResolvableConditionDecorableServiceDecorator1(ICollectionResolvableConditionDecorableService decoratee)
        {
            Decoratee = decoratee;
        }

        public ICollectionResolvableConditionDecorableService Decoratee { get; }
    }
}