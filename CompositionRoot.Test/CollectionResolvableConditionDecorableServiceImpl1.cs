namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Basics.Attributes;
    using Enumerations;

    [TestCondition1]
    [Lifestyle(EnLifestyle.Transient)]
    [Order(3)]
    public class CollectionResolvableConditionDecorableServiceImpl1 : ICollectionResolvableConditionDecorableService
    {
    }
}