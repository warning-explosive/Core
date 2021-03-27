namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Transient)]
    internal class CollectionResolvableConditionDecorableServiceDecorator2 : ICollectionResolvableConditionDecorableService,
                                                                             ICollectionResolvableConditionDecorableServiceDecorator<TestCondition2Attribute>
    {
        public CollectionResolvableConditionDecorableServiceDecorator2(ICollectionResolvableConditionDecorableService decoratee)
        {
            Decoratee = decoratee;
        }

        public ICollectionResolvableConditionDecorableService Decoratee { get; }
    }
}