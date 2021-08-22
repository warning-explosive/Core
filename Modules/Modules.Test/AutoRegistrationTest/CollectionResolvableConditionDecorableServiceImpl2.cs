namespace SpaceEngineers.Core.Modules.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;

    [TestCondition2]
    [Component(EnLifestyle.Transient)]
    [Dependency(typeof(CollectionResolvableConditionDecorableServiceImpl3))]
    internal class CollectionResolvableConditionDecorableServiceImpl2 : ICollectionResolvableConditionDecorableService
    {
    }
}