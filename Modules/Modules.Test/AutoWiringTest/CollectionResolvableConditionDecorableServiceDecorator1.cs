namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    internal class CollectionResolvableConditionDecorableServiceDecorator1 : ICollectionResolvableConditionDecorableService,
                                                                             ICollectionResolvableConditionDecorableServiceDecorator<TestCondition1Attribute>
    {
        public CollectionResolvableConditionDecorableServiceDecorator1(ICollectionResolvableConditionDecorableService decoratee)
        {
            Decoratee = decoratee;
        }

        public ICollectionResolvableConditionDecorableService Decoratee { get; }
    }
}