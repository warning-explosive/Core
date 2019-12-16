namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Basics.Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(1)]
    internal class CollectionResolvableConditionDecorableServiceImpl3 : ICollectionResolvableConditionDecorableService
    {
    }
}