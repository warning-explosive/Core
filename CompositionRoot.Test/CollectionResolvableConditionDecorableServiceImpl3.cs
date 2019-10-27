namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Enumerations;
    
    [Lifestyle(EnLifestyle.Transient)]
    [Order(1)]
    public class CollectionResolvableConditionDecorableServiceImpl3 : ICollectionResolvableConditionDecorableService
    {
    }
}