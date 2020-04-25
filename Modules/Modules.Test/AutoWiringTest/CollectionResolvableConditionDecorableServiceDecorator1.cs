namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class CollectionResolvableConditionDecorableServiceDecorator1 : ICollectionResolvableConditionDecorableServiceDecorator,
                                                                             ICollectionConditionalDecorator<ICollectionResolvableConditionDecorableService, TestCondition1Attribute>
    {
        public CollectionResolvableConditionDecorableServiceDecorator1(ICollectionResolvableConditionDecorableService decoratee)
        {
            Decoratee = decoratee;
        }

        public ICollectionResolvableConditionDecorableService Decoratee { get; }
    }
}