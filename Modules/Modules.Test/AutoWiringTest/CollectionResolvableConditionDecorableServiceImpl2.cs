namespace SpaceEngineers.Core.Modules.Test.AutoWiringTest
{
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics.Attributes;

    [TestCondition2]
    [Lifestyle(EnLifestyle.Transient)]
    [Dependency(typeof(CollectionResolvableConditionDecorableServiceImpl3))]
    internal class CollectionResolvableConditionDecorableServiceImpl2 : ICollectionResolvableConditionDecorableService
    {
    }
}