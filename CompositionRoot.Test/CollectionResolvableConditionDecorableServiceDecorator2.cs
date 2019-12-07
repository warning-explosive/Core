namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Abstractions;
    using Attributes;
    using Basics.Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(1)]
    public class CollectionResolvableConditionDecorableServiceDecorator2 : ICollectionResolvableConditionDecorableServiceDecorator,
                                                                           ICollectionConditionalDecorator<ICollectionResolvableConditionDecorableService, TestCondition2Attribute>
    {
        public CollectionResolvableConditionDecorableServiceDecorator2(ICollectionResolvableConditionDecorableService decoratee)
        {
            Decoratee = decoratee;
        }

        public ICollectionResolvableConditionDecorableService Decoratee { get; }
    }
}