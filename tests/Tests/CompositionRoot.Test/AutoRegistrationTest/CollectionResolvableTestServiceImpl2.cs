namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;

    [Component(EnLifestyle.Transient)]
    [After(typeof(CollectionResolvableTestServiceImpl3))]
    internal class CollectionResolvableTestServiceImpl2 : ICollectionResolvableTestService
    {
    }
}