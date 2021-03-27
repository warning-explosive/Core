namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics.Attributes;

    [Component(EnLifestyle.Transient)]
    [Dependency(typeof(CollectionResolvableTestServiceImpl2))]
    internal class CollectionResolvableTestServiceImpl1 : ICollectionResolvableTestService
    {
    }
}