namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics.Attributes;

    [TestCondition1]
    [Component(EnLifestyle.Transient)]
    [Dependency(typeof(CollectionResolvableConditionDecorableServiceImpl2))]
    internal class CollectionResolvableConditionDecorableServiceImpl1 : ICollectionResolvableConditionDecorableService
    {
    }
}