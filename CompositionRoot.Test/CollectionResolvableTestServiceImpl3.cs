namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Attributes;
    using Enumerations;
    using Basics.Attributes;

    [Lifestyle(EnLifestyle.Transient)]
    [Order(1)]
    internal class CollectionResolvableTestServiceImpl3 : ICollectionResolvableTestService
    {
    }
}