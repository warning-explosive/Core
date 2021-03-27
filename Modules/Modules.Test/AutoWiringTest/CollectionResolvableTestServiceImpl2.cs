namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics.Attributes;

    [Component(EnLifestyle.Transient)]
    [Dependency(typeof(CollectionResolvableTestServiceImpl3))]
    internal class CollectionResolvableTestServiceImpl2 : ICollectionResolvableTestService
    {
    }
}