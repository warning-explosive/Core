namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Enumerations;

    [TestConditionAttribute1]
    [Lifestyle(EnLifestyle.Transient)]
    [Order(3)]
    public class CollectionResolvableConditionDecorableServiceImpl1 : ICollectionResolvableConditionDecorableService
    {
    }
}