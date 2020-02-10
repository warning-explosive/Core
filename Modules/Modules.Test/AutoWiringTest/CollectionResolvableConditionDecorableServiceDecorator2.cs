namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class CollectionResolvableConditionDecorableServiceDecorator2 : ICollectionResolvableConditionDecorableServiceDecorator,
                                                                             ICollectionConditionalDecorator<ICollectionResolvableConditionDecorableService, TestCondition2Attribute>
    {
        public CollectionResolvableConditionDecorableServiceDecorator2(ICollectionResolvableConditionDecorableService decoratee)
        {
            Decoratee = decoratee;
        }

        public ICollectionResolvableConditionDecorableService Decoratee { get; }
    }
}