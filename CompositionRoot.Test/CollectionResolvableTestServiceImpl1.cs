namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Enumerations;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(3)]
    internal class CollectionResolvableTestServiceImpl1 : ICollectionResolvableTestService
    {
    }
}