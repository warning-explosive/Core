namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Enumerations;
    using Extensions.Attributes;

    [TestConditionAttribute2]
    [Lifestyle(EnLifestyle.Transient)]
    [Order(2)]
    public class CollectionResolvableConditionDecorableServiceImpl2 : ICollectionResolvableConditionDecorableService
    {
    }
}