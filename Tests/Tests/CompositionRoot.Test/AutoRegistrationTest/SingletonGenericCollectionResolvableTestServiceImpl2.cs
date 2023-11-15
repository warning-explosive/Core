namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;

    [Component(EnLifestyle.Singleton)]
    [After(typeof(SingletonGenericCollectionResolvableTestServiceImpl3<>))]
    internal class SingletonGenericCollectionResolvableTestServiceImpl2<T> : ISingletonGenericCollectionResolvableTestService<T>
    {
    }
}