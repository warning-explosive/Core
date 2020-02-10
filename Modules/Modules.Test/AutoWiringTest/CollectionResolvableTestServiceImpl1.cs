namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics.Attributes;

    [Lifestyle(EnLifestyle.Transient)]
    [Dependency(typeof(CollectionResolvableTestServiceImpl2))]
    internal class CollectionResolvableTestServiceImpl1 : ICollectionResolvableTestService
    {
    }
}