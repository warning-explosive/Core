namespace SpaceEngineers.Core.CompositionRoot.Test.AutoRegistrationTest
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics.Attributes;

    [Component(EnLifestyle.Singleton)]
    [After(typeof(SingletonGenericCollectionResolvableTestServiceImpl2<>))]
    internal class SingletonGenericCollectionResolvableTestServiceImpl1<T> : ISingletonGenericCollectionResolvableTestService<T>
    {
    }
}