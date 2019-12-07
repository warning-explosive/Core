namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Basics.Attributes;
    using Enumerations;

    [TestCondition2]
    [Lifestyle(EnLifestyle.Transient)]
    [Order(2)]
    public class CollectionResolvableConditionDecorableServiceImpl2 : ICollectionResolvableConditionDecorableService
    {
    }
}