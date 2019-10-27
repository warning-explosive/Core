namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Enumerations;
    
    [Lifestyle(EnLifestyle.Transient)]
    [Order(2)]
    internal class CollectionResolvableTestServiceImpl2 : ICollectionResolvableTestService
    {
    }
}